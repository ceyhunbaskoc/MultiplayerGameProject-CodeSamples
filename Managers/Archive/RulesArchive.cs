using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Managers.CoreLogic;
using Managers.Inspection;
using Managers.Network;
using TMPro;
using UnityEngine;

namespace Managers.Archive
{
    public static class RulesArchive
    {
        public static string GetAllRulesText()
        {
            List<Rule> allRules = RuleManager.GetAllRules();
            StringBuilder sb = new StringBuilder();
            foreach (Rule rule in allRules)
            {
                sb.AppendLine($"<b>{rule.ruleId}</b>: {rule.description}");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public static string GetActiveRulesText()
        {
            List<Rule> activeRules = RuleManager.GetActiveRules(CoopSyncManager.Instance.SyncedDayIndex.Value);
            StringBuilder sb = new StringBuilder();
            foreach (Rule rule in activeRules)
            {
                sb.AppendLine($"<b>{rule.ruleId}</b>: {rule.description}");
                sb.AppendLine();
            }
            return sb.ToString();
        }
        
        public static string GetRuleTextByCategory(RuleCategory category)
        {
            List<Rule> rules = RuleManager.GetRulesByCategory(category);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"<b>=== {category} ===</b>");
            sb.AppendLine();
            foreach (Rule rule in rules)
            {
                sb.AppendLine($"<b>{rule.ruleId}</b>: {rule.description}");
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public static string GetAllRulesGroupedByCategory()
        {
            List<RuleCategory> categories = RuleManager.GetAllCategories();
            StringBuilder sb = new StringBuilder();
            foreach (RuleCategory category in categories)
            {
                sb.AppendLine($"<b>=== {category} ===</b>");
                sb.AppendLine();
                List<Rule> rules = RuleManager.GetRulesByCategory(category);
                foreach (Rule rule in rules)
                {
                    sb.AppendLine($"<b>{rule.ruleId}</b>: {rule.description}");
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }

        public static string GetActiveRulesGroupedByCategory()
        {
            List<Rule> activeRules = RuleManager.GetActiveRules(CoopSyncManager.Instance.SyncedDayIndex.Value);
            List<RuleCategory> categories = activeRules
                .Select(r => r.categoryEnum)
                .Distinct()
                .ToList();
        
            StringBuilder sb = new StringBuilder();
            foreach (RuleCategory category in categories)
            {
                sb.AppendLine($"<b>=== {category} ===</b>");
                sb.AppendLine();
                foreach (Rule rule in activeRules.Where(r => r.categoryEnum == category))
                {
                    sb.AppendLine($"<b>{rule.ruleId}</b>: {rule.description}");
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }
    }
}