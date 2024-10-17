using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoteachApi.Models
{
    public class PreferencesRequest
    {
        public PreferencesRequest() 
        {
            Motivators = [];
            ProgrammingLanguages = [];
            Technologies = [];
        }

        public string? Name { get; set; }
        public string? AgeGroup { get; set; }
        public string? Location { get; set; }
        public string? Language { get; set; }
        public string? Education { get; set; }
        public string? Goals { get; set; }
        public string? LearningStyle { get; set; }
        public string? Pace { get; set; }
        public string? JobRole { get; set; }
        public string? SkillLevel { get; set; }
        public string? TimeAvailability { get; set; }
        public string? Schedule { get; set; }
        public string? GoogleId { get; set; }
        public List<string> Motivators { get; set; }
        public List<string> ProgrammingLanguages { get; set; }
        public List<string> Technologies { get; set; }
    }
}
