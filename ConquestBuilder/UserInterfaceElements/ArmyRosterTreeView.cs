using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using ConquestBuilder.ViewModels;
using ConquestController.Models;

namespace ConquestBuilder.UserInterfaceElements
{
    internal class ArmyRosterTreeView
    {
        private const int MAIN_NODE_INDEX = 0;
        private const int OPTIONS_NODE_INDEX = 1;
        private const int MAINSTAY_NODE_INDEX = 2;
        private const int RESTRICTED_NODE_INDEX = 3;

        public static void RefreshRosterTreeView(TreeView tvRoster, Guid selectedElementID, IEnumerable<RosterCharacter> characters)
        {
            //for right now we will go ahead and clear the tree out, repopulate it, and then select the item in the RosterChangedEventArgs
            //additionally make sure that they are always expanded
            tvRoster.Items.Clear();

            foreach (var character in characters)
            {
                var mainNodes = CreateMainNodeElements(character, selectedElementID);
                CreateMainstayNodeElements(character, selectedElementID, mainNodes[MAINSTAY_NODE_INDEX]);
                CreateRestrictedNodeElements(character, selectedElementID, mainNodes[RESTRICTED_NODE_INDEX]);
                CreateOptionNodeElements(character, mainNodes[OPTIONS_NODE_INDEX]);
                
                AssignNodesToTree(tvRoster, mainNodes);
            }

            //todo: make sure the selected item is set to keep the mainstay and restricted portraits loaded when you select from the treeview
        }

        private static void AssignNodesToTree(TreeView tvRoster, List<TreeViewItem> mainNodes)
        {
            mainNodes[MAIN_NODE_INDEX].Items.Add(mainNodes[OPTIONS_NODE_INDEX]);
            mainNodes[MAIN_NODE_INDEX].Items.Add(mainNodes[MAINSTAY_NODE_INDEX]);
            mainNodes[MAIN_NODE_INDEX].Items.Add(mainNodes[RESTRICTED_NODE_INDEX]);
            tvRoster.Items.Add(mainNodes[MAIN_NODE_INDEX]);
        }

        private static List<TreeViewItem> CreateMainNodeElements(RosterCharacter character, Guid selectedElementID)
        {
            var tvItem = new TreeViewItem()
            {
                Header = character.CharacterHeader,
                IsSelected = (character.Character.ID == selectedElementID),
                IsExpanded = true,
                Tag = new TreeViewRoster() { Category = RosterCategory.Character, Model = character, RosterCharacter = character }
            };

            var optionsNode = new TreeViewItem()
            {
                Header = "Options",
                IsSelected = false,
                IsExpanded = true,
                Tag = new TreeViewRoster() { Category = RosterCategory.OptionLabel, RosterCharacter = character }
            };

            var mainstayNode = new TreeViewItem()
            {
                Header = "Mainstay Regiments",
                IsSelected = false,
                IsExpanded = true,
                Tag = new TreeViewRoster() { Category = RosterCategory.MainstayLabel, RosterCharacter = character }
            };

            var restrictedNode = new TreeViewItem()
            {
                Header = "Restricted Regiments",
                IsSelected = false,
                IsExpanded = true,
                Tag = new TreeViewRoster() { Category = RosterCategory.RestrictedLabel, RosterCharacter = character }
            };

            var returnList = new List<TreeViewItem> {tvItem, optionsNode, mainstayNode, restrictedNode};
            return returnList;
        }

        private static void CreateMainstayNodeElements(RosterCharacter character, Guid selectedElementID,
            TreeViewItem mainstayNode)
        {
            foreach (var regiment in character.MainstayRegiments)
            {
                var mainstayRegiment = new TreeViewItem()
                {
                    Header = $"{regiment} - [x{regiment.StandCount}] - {regiment.TotalPoints} pts",
                    IsSelected = (regiment.ID == selectedElementID),
                    IsExpanded = true,
                    Tag = new TreeViewRoster() { Category = RosterCategory.MainstayRegiment, Model = regiment, RosterCharacter = character }
                };

                foreach (var option in regiment.ActiveOptions)
                {
                    var regimentOptions = new TreeViewItem()
                    {
                        Header = $"{option.Name} - {option.Points} pts",
                        IsSelected = false,
                        IsExpanded = true,
                        Tag = new TreeViewRoster() { Category = RosterCategory.Option, Model = option, RosterCharacter = character }
                    };

                    mainstayRegiment.Items.Add(regimentOptions);
                }

                mainstayNode.Items.Add(mainstayRegiment);
            }
        }

        private static void CreateRestrictedNodeElements(RosterCharacter character, Guid selectedElementID,
            TreeViewItem restrictedNode)
        {
            foreach (var regiment in character.RestrictedRegiments)
            {
                var restrictedRegiment = new TreeViewItem()
                {
                    Header = $"{regiment} - [x{regiment.StandCount}] - {regiment.TotalPoints} pts",
                    IsSelected = (regiment.ID == selectedElementID),
                    IsExpanded = true,
                    Tag = new TreeViewRoster() { Category = RosterCategory.RestrictedRegiment, Model = regiment, RosterCharacter = character }
                };

                foreach (var option in regiment.ActiveOptions)
                {
                    var regimentOptions = new TreeViewItem()
                    {
                        Header = $"{option} - {option.Points} pts",
                        IsSelected = false,
                        IsExpanded = true,
                        Tag = new TreeViewRoster() { Category = RosterCategory.Option, Model = option, RosterCharacter = character }
                    };

                    restrictedRegiment.Items.Add(regimentOptions);
                }

                restrictedNode.Items.Add(restrictedRegiment);
            }
        }

        private static void CreateOptionNodeElements(RosterCharacter character, TreeViewItem optionNode)
        {
            var allOptions = character.Character.ActiveOptions.ToList();
            allOptions.AddRange(character.Character.ActiveItems);
            allOptions.AddRange(character.Character.ActiveMasteries);
            allOptions.AddRange(character.Character.ActiveRetinues);
            allOptions.AddRange(character.Character.ActivePerks);

            foreach (var option in allOptions)
            {
                var characterOption = new TreeViewItem()
                {
                    Header = $"{option} - {option.Points} pts",
                    IsSelected = false,
                    IsExpanded = true,
                    Tag = new TreeViewRoster() { Category = RosterCategory.Option, Model = option, RosterCharacter = character }
                };

                optionNode.Items.Add(characterOption);
            }
        }
    }
}
