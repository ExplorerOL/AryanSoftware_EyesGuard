namespace EyesGuard.Data

// Importing modules or name spaces
open System.Globalization
open FSharp.Configuration
open System.Collections.ObjectModel
open System.Runtime.InteropServices

// New module declaration
module LanguageLoader =

    // Importing modules or name spaces
    open System
    open System.IO
    open CompilerExtensions

    // Default language = English
    let defaultLocale = "en-US"
    
    // Force compiling
    [<Literal>]
    //File for default language
    let defaultLocaleAddress = "Languages/en-US.yml"

    // Declaration of YAML Provider with default language
    type LocalizedEnvironment = YamlConfig<defaultLocaleAddress>

    // Identifying of directory with languages depending of mode (design or not)
    let localizationDirectoryPath designMode =
        if designMode then Path.Combine [| CompilerInfo.CompilerDirectory; "Languages" |]
        else Path.Combine [|AppDomain.CurrentDomain.BaseDirectory; "Languages" |]

    // Creating an array of available languages
    let supportedCultures =
        CultureInfo.GetCultures(CultureTypes.AllCultures)
        |> Array.map (fun x -> x.Name)
    
    // getLocalePath (locale , designMode) -> structure (path(to lang file) [string], locale (filename) [string])
    let getLocalePath locale designMode = Path.Combine [|localizationDirectoryPath designMode; sprintf "%s.yml" locale|]

    // Checking if the lang file exists
    let localeFileExists locale designMode =
        getLocalePath locale designMode
        |> File.Exists

    // Reading lang file content
    let getLocaleContent locale designMode =
        getLocalePath locale designMode |> File.ReadAllText

    // locale = file name. Cheching if the locale (en-EN) is supported in system
    let isCultureSupported locale = Array.contains locale supportedCultures

    // Language is supported and lang file exists
    let isCultureSupportedAndExists locale designMode =
        (localeFileExists locale designMode) && (isCultureSupported locale)

    
    let createEnvironment locale designmode =
        if isCultureSupportedAndExists locale designmode then
            let lang = LocalizedEnvironment()
            let path = getLocalePath locale designmode
            lang.Load path
            lang
        else
            LocalizedEnvironment()

    let defaultEnvironment = LocalizedEnvironment()

    type LanguageHolder = { Name : string; NativeName : string }

    let localeFiles designMode =
        Directory.GetFiles (localizationDirectoryPath designMode, "*.yml", SearchOption.TopDirectoryOnly)
        |> Array.map Path.GetFileNameWithoutExtension

    let languagesBriefData designMode =
        let items = localeFiles designMode
                    |> Array.filter isCultureSupported
                    |> Array.map CultureInfo
                    |> Array.map (fun x -> {
                        Name = x.Name
                        NativeName = x.NativeName
                    })

        lazy (items |> ObservableCollection<LanguageHolder>)

    // C# Object used to respect .NET conventions
    type FsLanguageLoader() =
        static member DefaultLocale = defaultLocale
        static member CreateEnvironment (locale, [<Optional;DefaultParameterValue(false)>]designMode) = createEnvironment locale designMode
        static member LanguagesBriefData ([<Optional;DefaultParameterValue(false)>]designMode)  = languagesBriefData designMode
        static member DefaultEnvironment = defaultEnvironment
        static member IsCultureSupportedAndExists (locale, [<Optional;DefaultParameterValue(false)>] designMode) = isCultureSupportedAndExists locale designMode
