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
   
    //File for default language with local path
    let defaultLocaleAddress = "Languages/en-US.yml"

    // Generating a data structure containing fields from YAML file (needs YAMLConfig Package)
    type LocalizedEnvironment = YamlConfig<defaultLocaleAddress>

    // Identifying of the directory with languages depending of mode (design or not); Combines two strings into a path
    let localizationDirectoryPath designMode =
        if designMode then Path.Combine [| CompilerInfo.CompilerDirectory; "Languages" |]
        else Path.Combine [|AppDomain.CurrentDomain.BaseDirectory; "Languages" |]

    // Gets the list of supported cultures filtered by the specified CultureTypes parameter
    let supportedCultures =
        CultureInfo.GetCultures(CultureTypes.AllCultures)
        |> Array.map (fun x -> x.Name)
    
    // Generating the path to lang file  of type string [Path to directory + fileName]; Combines two strings into a path
    let getLocalePath locale designMode = Path.Combine [|localizationDirectoryPath designMode; sprintf "%s.yml" locale|]

    // Checking if the lang file exists (if file exists 1 is returned)
    let localeFileExists locale designMode =
        getLocalePath locale designMode
        |> File.Exists

    // Reading lang file content into a string and closing the file
    let getLocaleContent locale designMode =
        getLocalePath locale designMode |> File.ReadAllText

    // locale = file name. Cheching if the locale (en-EN) is supported in system
    let isCultureSupported locale = Array.contains locale supportedCultures

    // Language is supported and lang file exists
    let isCultureSupportedAndExists locale designMode =
        (localeFileExists locale designMode) && (isCultureSupported locale)

    // 
    let createEnvironment locale designmode =
        if isCultureSupportedAndExists locale designmode then   // Cheching if language exists and supported
            let lang = LocalizedEnvironment()                   // Definition of lang (lang = YAML provider)
            let path = getLocalePath locale designmode          // Definition of path (set path depending on design mode)
            lang.Load path                                      // load YAML file path
            lang                                                // Return YAML provider with appropriate lang path
        else
            LocalizedEnvironment()                              // Else return YAML provider with appropriate default lang path

    let defaultEnvironment = LocalizedEnvironment()             // Return YAML provider with appropriate default lang path
    
    
    type LanguageHolder = { Name : string; NativeName : string }    // Creating type LanguageHolder

    // Creating an array of available languages for application
    let localeFiles designMode =
        Directory.GetFiles (localizationDirectoryPath designMode, "*.yml", SearchOption.TopDirectoryOnly)
        |> Array.map Path.GetFileNameWithoutExtension

    // items array element = [Name; NativeName]
    let languagesBriefData designMode =
        let items = localeFiles designMode
                    |> Array.filter isCultureSupported
                    |> Array.map CultureInfo
                    |> Array.map (fun x -> {
                        Name = x.Name
                        NativeName = x.NativeName
                    })
    // delayed processing: creating a collection of languages
        lazy (items |> ObservableCollection<LanguageHolder>)

    // C# Object used to respect .NET conventions
    type FsLanguageLoader() =
        static member DefaultLocale = defaultLocale     // Returns string with defaultLocale = "en-US"
        static member CreateEnvironment (locale, [<Optional;DefaultParameterValue(false)>]designMode) = createEnvironment locale designMode     // create YAML provider for appropriate lang
        static member LanguagesBriefData ([<Optional;DefaultParameterValue(false)>]designMode)  = languagesBriefData designMode                 // create array element = [LangName; LangNativeName]
        static member DefaultEnvironment = defaultEnvironment                                                                                   // create YAML provider for default (English) language  (defaultEnvironment = LocalizedEnvironment())
        static member IsCultureSupportedAndExists (locale, [<Optional;DefaultParameterValue(false)>] designMode) = isCultureSupportedAndExists locale designMode    // language file exists ans is supported
