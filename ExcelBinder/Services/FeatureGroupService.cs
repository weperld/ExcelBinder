using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExcelBinder.Models;
using Newtonsoft.Json;

namespace ExcelBinder.Services
{
    public struct GroupOperationResult
    {
        public bool Success;
        public string? ErrorMessage;
        public FeatureGroup? Group;

        public static GroupOperationResult Ok(FeatureGroup? group = null) =>
            new() { Success = true, Group = group };

        public static GroupOperationResult Fail(string message) =>
            new() { Success = false, ErrorMessage = message };
    }

    public class FeatureGroupService
    {
        private string _featuresDirectory;

        public FeatureGroupService(string featuresDirectory)
        {
            _featuresDirectory = featuresDirectory ?? string.Empty;
        }

        public void UpdateFeaturesDirectory(string featuresDirectory)
        {
            _featuresDirectory = featuresDirectory ?? string.Empty;
        }

        private string GroupsFilePath =>
            Path.Combine(_featuresDirectory, ProjectConstants.Defaults.GroupsFileName);

        public List<FeatureGroup> LoadGroups()
        {
            if (string.IsNullOrEmpty(_featuresDirectory) || !File.Exists(GroupsFilePath))
                return new List<FeatureGroup>();

            try
            {
                var json = File.ReadAllText(GroupsFilePath);
                var collection = JsonConvert.DeserializeObject<FeatureGroupCollection>(json);
                return collection?.Groups ?? new List<FeatureGroup>();
            }
            catch (Exception ex)
            {
                LogService.Instance.Warning($"Failed to load groups from {GroupsFilePath}: {ex.Message}");
                return new List<FeatureGroup>();
            }
        }

        public void SaveGroups(IEnumerable<FeatureGroup> groups)
        {
            if (string.IsNullOrEmpty(_featuresDirectory)) return;

            try
            {
                if (!Directory.Exists(_featuresDirectory))
                    Directory.CreateDirectory(_featuresDirectory);

                var persisted = groups.Where(g => !g.IsAllGroup).ToList();
                var collection = new FeatureGroupCollection { Groups = persisted };
                var json = JsonConvert.SerializeObject(collection, Formatting.Indented);
                File.WriteAllText(GroupsFilePath, json);
            }
            catch (Exception ex)
            {
                LogService.Instance.Error($"Failed to save groups to {GroupsFilePath}: {ex.Message}");
            }
        }

        public GroupOperationResult AddGroup(string name, IList<FeatureGroup> existing)
        {
            string? error = ValidateName(name, existing, currentId: null);
            if (error != null) return GroupOperationResult.Fail(error);

            var group = new FeatureGroup
            {
                Id = Guid.NewGuid().ToString(),
                Name = name.Trim(),
                FeatureIds = new List<string>()
            };
            existing.Add(group);
            SaveGroups(existing);
            return GroupOperationResult.Ok(group);
        }

        public GroupOperationResult RenameGroup(string id, string newName, IList<FeatureGroup> existing)
        {
            var target = existing.FirstOrDefault(g => g.Id == id);
            if (target == null) return GroupOperationResult.Fail("그룹을 찾을 수 없습니다.");
            if (target.IsAllGroup) return GroupOperationResult.Fail("'전체' 그룹은 이름을 변경할 수 없습니다.");

            string? error = ValidateName(newName, existing, currentId: id);
            if (error != null) return GroupOperationResult.Fail(error);

            target.Name = newName.Trim();
            SaveGroups(existing);
            return GroupOperationResult.Ok(target);
        }

        public bool DeleteGroup(string id, IList<FeatureGroup> existing)
        {
            var target = existing.FirstOrDefault(g => g.Id == id);
            if (target == null || target.IsAllGroup) return false;

            existing.Remove(target);
            SaveGroups(existing);
            return true;
        }

        public bool ToggleFeatureInGroup(string groupId, string featureId, IList<FeatureGroup> existing)
        {
            var target = existing.FirstOrDefault(g => g.Id == groupId);
            if (target == null || target.IsAllGroup) return false;

            if (target.FeatureIds.Contains(featureId))
                target.FeatureIds.Remove(featureId);
            else
                target.FeatureIds.Add(featureId);

            SaveGroups(existing);
            return true;
        }

        public void CleanupDanglingIds(IEnumerable<string> validFeatureIds, IList<FeatureGroup> groups)
        {
            var valid = new HashSet<string>(validFeatureIds);
            bool dirty = false;
            foreach (var g in groups.Where(g => !g.IsAllGroup))
            {
                int removed = g.FeatureIds.RemoveAll(id => !valid.Contains(id));
                if (removed > 0) dirty = true;
            }
            if (dirty) SaveGroups(groups);
        }

        private string? ValidateName(string name, IEnumerable<FeatureGroup> existing, string? currentId)
        {
            if (string.IsNullOrWhiteSpace(name))
                return ProjectConstants.UI.MsgGroupNameRequired;

            string trimmed = name.Trim();
            bool duplicate = existing.Any(g =>
                !g.IsAllGroup &&
                g.Id != currentId &&
                string.Equals(g.Name, trimmed, StringComparison.OrdinalIgnoreCase));

            if (duplicate)
                return ProjectConstants.UI.MsgGroupNameDuplicate;

            if (string.Equals(trimmed, ProjectConstants.Defaults.AllGroupName, StringComparison.OrdinalIgnoreCase))
                return ProjectConstants.UI.MsgGroupNameDuplicate;

            return null;
        }
    }
}
