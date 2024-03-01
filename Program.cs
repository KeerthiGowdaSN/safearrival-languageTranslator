using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Resources.NetStandard;
using System.Xml.Linq;
using Google.Cloud.Translation.V2;
using Google.Cloud.Language.V1;
using Google.Api.Gax.Grpc;
using System.Resources;
using static Grpc.Core.Metadata;
using Google.Protobuf.WellKnownTypes;
using System.Diagnostics;

class Program
{
    static void Main()
    {
        try
        {
            Console.Title = "LTK Forge";
            Console.WriteLine("Welcome to the LTK Forge! ");
            Console.WriteLine("\nYour multi-language resource file generator.");
            Console.WriteLine("\nInput resource file path:");
            string inputResourceFile = Console.ReadLine();

            // Your Google Cloud API key
            string googleApiKey = "AIzaSyDpry3bKi50-yI3rOMRBO6051CFg6GAuKA";

            Dictionary<string, string> outputResources;
            Dictionary<string, string> inputResources;

            Console.WriteLine("\nEnter the language code(s) (separated by commas) to generate the resource files.");
            string targetLanguages = Console.ReadLine();
            var languageCodes = targetLanguages.Split(',');


            foreach (var targetLanguage in languageCodes)
            {
                string outputResourceFile = "C:\\Users\\snkee\\Downloads\\LanguageTranslator\\LanguageConverter\\SharedResource." + targetLanguage.Trim() + ".resx";
                if (!File.Exists(outputResourceFile))
                {
                    string defaultResxFilePath = "C:\\Users\\snkee\\Downloads\\LanguageTranslator\\LanguageConverter\\Sample\\Resource1.resx";
                    //copy defaultResxFilePath to outputResourceFile
                    File.Copy(defaultResxFilePath, outputResourceFile);

                }
                inputResources = ReadResourceFile(inputResourceFile, outputResourceFile, out outputResources);

                // Translate values to the target language using Google Cloud Translation API
                TranslateValues(inputResources, outputResources, googleApiKey, targetLanguage.Trim().Split('-')[0], outputResourceFile);
            }

            Console.WriteLine("Translation completed successfully.");
            Console.WriteLine("Translated resource files are saved under '" + Path.GetDirectoryName(inputResourceFile) + "'" );

            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    static Dictionary<string, string> ReadResourceFile(string inputFilePath, string outputFilePath, out Dictionary<string, string> outputResources)
    {
        outputResources = new Dictionary<string, string>();
        Dictionary<string, string> inputResources = new Dictionary<string, string>();
        //Dictionary<string, string> outputResources = new Dictionary<string, string>();
        using (var reader = new ResXResourceReader(inputFilePath))
        {
            foreach (DictionaryEntry entry in reader)
            {
                inputResources.Add(entry.Key.ToString(), entry.Value.ToString());
            }
        }

        using (var reader = new ResXResourceReader(outputFilePath))
        {
            foreach (DictionaryEntry entry in reader)
            {
                outputResources.Add(entry.Key.ToString(), entry.Value.ToString());
            }
        }
        return inputResources;
    }

    static void TranslateValues(Dictionary<string, string> inputResources, Dictionary<string, string> targetResources, string apiKey, string targetLanguage, string outputResourceFile)
    {
        TranslationClient client = TranslationClient.CreateFromApiKey(apiKey);
        //List<string> keysToTranslate = new List<string>(inputResources.Keys);
        // Load the existing resource file
        XDocument doc = XDocument.Load(outputResourceFile);
        foreach (var entry in inputResources)
        {
            if (!KeyExistsInResourceFile(targetResources, entry.Key))
            {
                var translation = client.TranslateText(inputResources[entry.Key], targetLanguage);
                string value = translation.TranslatedText;
                XElement dataElement = new XElement("data",
                   new XAttribute("name", entry.Key),
                   new XElement("value", value)
               );
                doc.Root.Add(dataElement);
            }
        }
        // Save the updated XML document back to the resource file
        doc.Save(outputResourceFile);
    }

    static bool KeyExistsInResourceFile(Dictionary<string, string> outputResources, string key)
    {
        try
        {
            if (outputResources.ContainsKey(key))
            {
                return true;
            }
            else return false;
        }
        catch (Exception ex)
        {
            // Handle any exceptions (e.g., file not found, invalid resource file format, etc.)
            Console.WriteLine($"Error accessing resource file: {ex.Message}");
        }

        return false;
    }


}

