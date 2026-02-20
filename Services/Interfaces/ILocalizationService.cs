using AutoTranslator.Models.Enums;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace AutoTranslator.Services.Interfaces;

public interface ILocalizationService : INotifyPropertyChanged
{
    public string this[string key] { get; }
    public void SetLanguage(string language);
}
