using AutoTranslator.Models;
using AutoTranslator.Models.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoTranslator.Services.Interfaces;

public interface IProjectService
{
    public Task<Project> CreateProjectAsync(ProjectCreateDTO createDTO);
    public Task<Project> LoadProjectAsync(string projectFilePath);
    public Task SaveProjectAsync(Project project);
    public Task DeleteProjectAsync(Project project);
    public Task<List<Project>> GetAllProjectAsync();
}