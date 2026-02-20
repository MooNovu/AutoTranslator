using AutoTranslator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTranslator.Services.Interfaces;

public interface ISettingsService
{
    public AppSettings Settings { get; }
    public Task LoadAsync();
    public Task SaveAsync();
}
