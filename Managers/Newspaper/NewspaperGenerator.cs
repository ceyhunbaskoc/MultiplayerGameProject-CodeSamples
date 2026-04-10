using System.Collections.Generic;
using System.Text;
using Managers.Inspection;

namespace Managers.Newspaper
{
    public static class NewspaperGenerator
    {
        public static string GenerateTodaysNews(int currentDayIndex)
        {
            List<Rule> todaysRules = RuleManager.GetActiveRules(currentDayIndex);
            
            List<Rule> yesterdaysRules = currentDayIndex > 1 
                ? RuleManager.GetActiveRules(currentDayIndex - 1) 
                : new List<Rule>();

            StringBuilder newsBuilder = new StringBuilder();
            bool hasNews = false;

            foreach (Rule rule in todaysRules)
            {
                if (!yesterdaysRules.Exists(r => r.ruleId == rule.ruleId))
                {
                    if (!string.IsNullOrWhiteSpace(rule.newsOnActivate))
                    {
                        newsBuilder.AppendLine($"• {rule.newsOnActivate}\n");
                        hasNews = true;
                    }
                }
            }

            foreach (Rule rule in yesterdaysRules)
            {
                if (!todaysRules.Exists(r => r.ruleId == rule.ruleId))
                {
                    if (!string.IsNullOrWhiteSpace(rule.newsOnDeactivate))
                    {
                        newsBuilder.AppendLine($"• {rule.newsOnDeactivate}\n");
                        hasNews = true;
                    }
                }
            }

            if (!hasNews)
            {
                newsBuilder.AppendLine("A casual day.");
                newsBuilder.AppendLine("The government has not issued a new statement.");
            }

            return newsBuilder.ToString();
        }
    }
}