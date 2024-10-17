using HoteachApi.Models;
using System.Text;

namespace HoteachApi.Contracts.Helpers
{
    public static class GeneratePrompt
    {
        public static string SystemMessage()
        {
            return "Welcome to our Advanced Learning Path Generator! " +
                "We are a cutting-edge platform designed to empower learners by " +
                "creating personalized educational experiences tailored to your unique " +
                "needs and aspirations. Leveraging the latest advancements in AI and learning sciences, " +
                "we help you identify the optimal path through a wide array of programming languages, " +
                "technologies, and career-specific skills. Whether you are starting your journey in tech, " +
                "aiming to upscale your expertise, or shifting career paths, our AI-driven guidance system " +
                "will continually adapt to your learning pace, preferences, and goals. Please share your learning " +
                "objectives, and let's embark on a journey towards mastery, efficiency, and success in your chosen field. " +
                "Our platform not only suggests relevant materials and exercises but also adjusts dynamically " +
                "to your evolving competences, ensuring you stay motivated and engaged. Discover the future of " +
                "personalized learning today and transform how you acquire and apply knowledge and skills in real-world scenarios."
        }

        public static string UserMessage(UserPreferences preferences)
        {
            StringBuilder requestBuilder = new StringBuilder();
            requestBuilder.Append("Hello! I'm looking to develop a comprehensive learning path tailored to my specific needs and goals. " +
                "Here are my preferences:\n");
            requestBuilder.AppendFormat("Name: {0}\n", preferences.Name ?? "Not specified");
            requestBuilder.AppendFormat("Age Group: {0}\n", preferences.AgeGroup ?? "Not specified");
            requestBuilder.AppendFormat("Location: {0}\n", preferences.Location ?? "Not specified");
            requestBuilder.AppendFormat("Preferred Language: {0}\n", preferences.Language ?? "Not specified");
            requestBuilder.AppendFormat("Education Background: {0}\n", preferences.Education ?? "Not specified");
            requestBuilder.AppendFormat("Career Goals: {0}\n", preferences.Goals ?? "Not specified");
            requestBuilder.AppendFormat("Learning Style: {0}\n", preferences.LearningStyle ?? "Not specified");
            requestBuilder.AppendFormat("Preferred Pace: {0}\n", preferences.Pace ?? "Not specified");
            requestBuilder.AppendFormat("Current Job Role: {0}\n", preferences.JobRole ?? "Not specified");
            requestBuilder.AppendFormat("Skill Level: {0}\n", preferences.SkillLevel ?? "Not specified");
            requestBuilder.AppendFormat("Time Availability: {0}\n", preferences.TimeAvailability ?? "Not specified");
            requestBuilder.AppendFormat("Usual Schedule: {0}\n", preferences.Schedule ?? "Not specified");
            requestBuilder.AppendLine("Here are the motivators that keep me engaged:");
            preferences.Motivators.ForEach(motivator => requestBuilder.AppendFormat("- {0}\n", motivator));
            requestBuilder.AppendLine("I am interested in the following programming languages:");
            preferences.ProgrammingLanguages.ForEach(language => requestBuilder.AppendFormat("- {0}\n", language));
            requestBuilder.AppendLine("I would like to delve into the following technologies:");
            preferences.Technologies.ForEach(technology => requestBuilder.AppendFormat("- {0}\n", technology));

            requestBuilder.Append("Based on the above, could you generate a personalized learning path that aligns with my career " +
                "aspirations and learning preferences? I'm looking for guidance on what steps to take next, suggested resources, " +
                "and a timeline that fits my availability. Thank you!");

            return requestBuilder.ToString();
        }
    }
}
