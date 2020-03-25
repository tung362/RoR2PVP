using UnityEngine;

namespace APIExtension.VoteAPI
{
    public struct ChoiceMenu
    {
        public string TooltipName;
        public Color TooltipNameColor;
        public string TooltipBody;
        public Color TooltipBodyColor;
        public string IconPath;
        public string MaterialPath;
        public string UnlockableName;
        public int ChoiceIndex;

        public ChoiceMenu(string tooltipName, Color tooltipNameColor, string tooltipBody, Color tooltipBodyColor, string iconPath, string materialPath, string unlockableName, int choiceIndex)
        {
            TooltipName = tooltipName;
            TooltipNameColor = tooltipNameColor;
            TooltipBody = tooltipBody;
            TooltipBodyColor = tooltipBodyColor;
            IconPath = iconPath;
            MaterialPath = materialPath;
            UnlockableName = unlockableName;
            ChoiceIndex = choiceIndex;
        }
    }
}
