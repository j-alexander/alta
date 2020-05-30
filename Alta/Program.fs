// Learn more about F# at http://fsharp.org

open System
open System.IO
open OpenQA.Selenium

let firefox _ =
    let firefox = new FileInfo(@"C:\Program Files\Mozilla Firefox\firefox.exe")
    let driver =
        let service = Firefox.FirefoxDriverService.CreateDefaultService(firefox.Directory.FullName, Host="::1")
        let options = new Firefox.FirefoxOptions()
        options.BrowserExecutableLocation <- firefox.FullName
        options.LogLevel <- Firefox.FirefoxDriverLogLevel.Trace
        options.AddArgument("-headless")
        options.SetPreference("security.sandbox.content.level", 5)
        new Firefox.FirefoxDriver(service, options)
    driver.Manage().Timeouts().ImplicitWait <- TimeSpan.FromSeconds 5.
    driver
    :> IWebDriver

let chrome _ =
    let path = @"C:\Program Files (x86)\Google\Chrome\Application\"
    
    let options = new Chrome.ChromeOptions()
    options.AddArgument("--headless")
    let driver = new Chrome.ChromeDriver(path, options)
    driver.Manage().Timeouts().ImplicitWait <- TimeSpan.FromSeconds 10.
    driver
    :> IWebDriver

type REPLBuffer = {
    Url : string option
    File : string option
    Input : string option
    XPath : string option }
with
    static member New =
        { Url = None
          File = None
          Input = None
          XPath = None }

[<EntryPoint>]
let main argv =

    use driver = 
        match argv with
        | [| "firefox" |] -> firefox()
        | [| "chrome" |] -> chrome()
        | x ->
            printfn "Driver not specified, using \"firefox\"."
            printfn ""
            printfn "i.e.> Alta.exe firefox"
            printfn ""
            firefox()

    let rec loop(buffer:REPLBuffer) =

        let input =
            Console.ReadLine()
        let command =
            input.Split([|' '|])
            |> List.ofArray
            |> List.filter (String.IsNullOrWhiteSpace >> not)
            |> List.map (fun x -> x.Trim())

        try
            match command, buffer with

            | [ "url"; url ], _ ->
                match Uri.TryCreate(url, UriKind.Absolute) with
                | true, _ ->
                    loop { buffer with Url=Some url }
                | false, _ ->
                    printfn "Malformed url, try again."
                    loop buffer

            | [ "file"; file ], _ ->
                loop { buffer with File=Some file }

            |   "input" :: _ :: _, _ ->
                loop { buffer with Input=Some(input.Substring(6)) }

            |   "xpath" :: _ :: _, _ ->
                loop { buffer with XPath=Some(input.Substring(6)) }

            | [ "load" ], { Url=Some url } ->
                driver.Navigate().GoToUrl(url)
                loop buffer

            | [ "load" ], { Url=url } ->
                printfn "Needs Data:"
                if url.IsNone then
                    printfn "  url"
                loop buffer

            | [ "save" ], { File=Some file } ->
                File.WriteAllText(file, driver.PageSource)
                loop buffer

            | [ "save" ], { File=file } ->
                printfn "Needs Data:"
                if file.IsNone then
                    printfn "  file"
                loop buffer

            | [ "dom" ], { XPath=Some xpath } ->
                let element = driver.FindElement(By.XPath xpath)
                let dom = element.GetAttribute("innerHTML")
                printfn "%s" dom
                loop buffer

            | [ "dom" ], { XPath=xpath } ->
                printfn "Needs Data:"
                if xpath.IsNone then
                    printfn "  xpath"
                loop buffer

            | [ "text" ], { XPath=Some xpath } ->
                let element = driver.FindElement(By.XPath xpath)
                let text = element.Text
                printfn "%s" text
                loop buffer

            | [ "text" ], { XPath=xpath } ->
                printfn "Needs Data:"
                if xpath.IsNone then
                    printfn "  xpath"
                loop buffer

            | [ "click" ], { XPath=Some xpath } ->
                let element = driver.FindElement(By.XPath xpath)
                element.Click()
                loop buffer

            | [ "click" ], { XPath=xpath } ->
                printfn "Needs Data:"
                if xpath.IsNone then
                    printfn "  xpath"
                loop buffer

            | [ "type" ], { Input=Some input; XPath=Some xpath } ->
                let element = driver.FindElement(By.XPath xpath)
                element.SendKeys(input)
                loop buffer

            | [ "type" ], { Input=input; XPath=xpath } ->
                printfn "Needs Data:"
                if input.IsNone then
                    printfn "  input"
                if xpath.IsNone then
                    printfn "  xpath"
                loop buffer

            | [ "reload" ], _ | [ "refresh" ], _ ->
                driver.Navigate().Refresh()
                loop buffer

            | [ "clear" ], _ ->
                loop REPLBuffer.New

            | [ "clear"; "url" ], _ ->
                loop { buffer with Url=None }

            | [ "clear"; "file" ], _ ->
                loop { buffer with File=None }

            | [ "clear"; "input" ], _ ->
                loop { buffer with Input=None }

            | [ "clear"; "xpath" ], _ ->
                loop { buffer with XPath=None }

            | [ "quit" ] , _ | [ "exit" ] , _ -> ()

            | _, buffer ->
                printfn ""
                printfn "Unrecognized Data or Command"
                printfn ""
                printfn "Data:"
                printfn "  url"
                printfn "  file"
                printfn "  input"
                printfn "  xpath"
                printfn ""
                printfn "Commands:"
                printfn "  load(url)"
                printfn "  save(file)"
                printfn "  dom(xpath)"
                printfn "  text(xpath)"
                printfn "  text(xpath)"
                printfn "  click(xpath)"
                printfn "  type(input,xpath)"
                printfn "  clear [url|text|file|xpath]"
                printfn "  reload|refresh"
                printfn "  quit|exit"
                printfn ""
                printfn "Buffer:"
                printfn "  url %s" (buffer.Url |> Option.defaultValue "<no url>")
                printfn "  file %s" (buffer.File |> Option.defaultValue "<no file>")
                printfn "  input %s" (buffer.Input |> Option.defaultValue "<no input>")
                printfn "  xpath %s" (buffer.XPath |> Option.defaultValue "<no xpath>")
                printfn ""
                loop buffer
        with e ->
            printfn "Error: %O" e.Message
            loop buffer

    printfn "Enter Data or Command:"
    loop(REPLBuffer.New)
    0
