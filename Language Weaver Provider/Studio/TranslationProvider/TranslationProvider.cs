﻿using System;
using System.Linq;
using System.Collections.Generic;
using LanguageWeaverProvider.LanguageMappingProvider;
using LanguageWeaverProvider.Model.Interface;
using LanguageWeaverProvider.Model.Options;
using Newtonsoft.Json;
using Sdl.LanguagePlatform.Core;
using Sdl.LanguagePlatform.TranslationMemoryApi;

namespace LanguageWeaverProvider
{
    public interface ITranslationProviderExtension
    {
        public Dictionary<string, string> LanguagesSupported { get; set; }
    }

    internal class TranslationProvider : ITranslationProvider, ITranslationProviderExtension
	{
		public TranslationProvider(ITranslationOptions translationOptions)
		{
			TranslationOptions = translationOptions;
			_ = DatabaseControl.InitializeDatabase();
            SetSupportedLanguages();
		}

		public string Name => TranslationOptions.ProviderName;

		public ITranslationOptions TranslationOptions { get; set; }

		public TranslationMethod TranslationMethod => TranslationMethod.MachineTranslation;

		public bool SupportsSearchForTranslationUnits => true;
		
		public bool SupportsTaggedInput => true;
		
		public bool SupportsTranslation => true;
		
		public bool SupportsPenalties => true;
		
		public bool SupportsScoring => true;
		
		public bool IsReadOnly => true;
		
		public bool SupportsConcordanceSearch => false;
		
		public bool SupportsDocumentSearches => false;
		
		public bool SupportsSourceConcordanceSearch => false;
		
		public bool SupportsTargetConcordanceSearch => false;
		
		public bool SupportsStructureContext => false;
		
		public bool SupportsMultipleResults => false;
		
		public bool SupportsFuzzySearch => false;
		
		public bool SupportsPlaceables => false;
		
		public bool SupportsWordCounts => false;
		
		public bool SupportsFilters => false;
		
		public bool SupportsUpdate => false;

		public ProviderStatusInfo StatusInfo => new(true, Constants.PluginName);

		public Uri Uri => TranslationOptions.Uri;

        public Dictionary<string, string> LanguagesSupported { get; set; } = new();

        public void LoadState(string translationProviderState)
		{
			var translationOptions = JsonConvert.DeserializeObject<TranslationOptions>(translationProviderState);
			TranslationOptions = translationOptions;
		}
		public string SerializeState()
		{
			var translationProviderState = JsonConvert.SerializeObject(TranslationOptions);
			return translationProviderState;
		}

		public bool SupportsLanguageDirection(LanguagePair languageDirection)
		{
			var currentPair = TranslationOptions.PairMappings.FirstOrDefault(pair => pair.LanguagePair.SourceCulture.Name == languageDirection.SourceCulture.Name
																				  && pair.LanguagePair.TargetCulture.Name == languageDirection.TargetCulture.Name);
			return currentPair is not null && !string.IsNullOrEmpty(currentPair.SelectedModel.Model);
		}

		public ITranslationProviderLanguageDirection GetLanguageDirection(LanguagePair languageDirection)
		{
			return new TranslationProviderLanguageDirection(this, TranslationOptions, languageDirection);
		}

        public void SetSupportedLanguages()
        {
            var options = TranslationOptions;
            var mappings = options.PairMappings;
            var name = options.PluginVersion == PluginVersion.LanguageWeaverEdge
                ? PluginResources.LCEdge_ShortName
                : PluginResources.LCCloud_ShortName;

            foreach (var mapping in mappings)
            {
                var languagePair = mapping.LanguagePair;
                var targetLanguage = languagePair.TargetCulture.RegionNeutralName.ToUpper();
                if (!LanguagesSupported.ContainsKey(targetLanguage))
                {
                    if (!LanguagesSupported.ContainsKey(languagePair.TargetCultureName))
                    {
                        LanguagesSupported.Add(languagePair.TargetCultureName, name);
                    }
                }
            }

        }

		public void RefreshStatusInfo() { }
	}
}