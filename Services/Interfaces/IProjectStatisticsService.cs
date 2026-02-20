using AutoTranslator.Models;
using System.Threading.Tasks;

namespace AutoTranslator.Services.Interfaces;

public interface IProjectStatisticsService
{
    public Task UpdateAsync(Project project);
}
