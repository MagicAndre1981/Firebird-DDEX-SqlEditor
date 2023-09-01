// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.WPF.DialogColorProvider

using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using BlackbirdSql.Common.Enums;

namespace BlackbirdSql.Common.Model;


[EditorBrowsable(EditorBrowsableState.Never)]
public class DialogColorProvider
{
	public virtual Color GetColor(EnResourceKeyId id)
	{
		return id switch
		{
			EnResourceKeyId.ActionLinkColor => Color.FromArgb(byte.MaxValue, 0, 112, 192),
			EnResourceKeyId.ActionLinkDisabledColor => SystemColors.GrayTextColor,
			EnResourceKeyId.ActionLinkItemColor => Color.FromArgb(byte.MaxValue, 0, 112, 192),
			EnResourceKeyId.ActionLinkItemDisabledColor => SystemColors.GrayTextColor,
			EnResourceKeyId.ActionLinkItemHoverColor => Color.FromArgb(byte.MaxValue, 0, 112, 192),
			EnResourceKeyId.ActionLinkItemSelectedColor => Color.FromArgb(byte.MaxValue, 0, 112, 192),
			EnResourceKeyId.ActionLinkItemSelectedNotFocusedColor => Color.FromArgb(byte.MaxValue, 0, 112, 192),
			EnResourceKeyId.ArrowGlyphColor => SystemColors.WindowTextColor,
			EnResourceKeyId.ArrowGlyphMouseOverColor => SystemColors.HotTrackColor,
			EnResourceKeyId.BodyTextColor => SystemColors.WindowTextColor,
			EnResourceKeyId.ButtonBackgroundColor => SystemColors.ControlColor,
			EnResourceKeyId.ButtonBorderColor => SystemColors.ControlColor,
			EnResourceKeyId.ButtonDisabledBackgroundColor => SystemColors.ControlColor,
			EnResourceKeyId.ButtonDisabledBorderColor => SystemColors.ControlColor,
			EnResourceKeyId.ButtonDisabledForegroundColor => SystemColors.ControlTextColor,
			EnResourceKeyId.ButtonForegroundColor => SystemColors.ControlTextColor,
			EnResourceKeyId.ButtonHoverBackgroundColor => SystemColors.ControlColor,
			EnResourceKeyId.ButtonHoverBorderColor => SystemColors.ControlColor,
			EnResourceKeyId.ButtonHoverForegroundColor => SystemColors.ControlTextColor,
			EnResourceKeyId.ButtonPressedBackgroundColor => SystemColors.ControlColor,
			EnResourceKeyId.ButtonPressedBorderColor => SystemColors.ControlColor,
			EnResourceKeyId.ButtonPressedForegroundColor => SystemColors.ControlTextColor,
			EnResourceKeyId.DefaultSelectedAnchorPointBorderColor => Color.FromArgb(byte.MaxValue, 0, 52, 171),
			EnResourceKeyId.DefaultSelectedAnchorPointColor => Color.FromArgb(87, byte.MaxValue, 246, 204),
			EnResourceKeyId.DefaultUnselectedAnchorPointBorderColor => Color.FromArgb(byte.MaxValue, 204, 153, 51),
			EnResourceKeyId.DefaultUnselectedAnchorPointColor => Color.FromArgb(87, byte.MaxValue, 237, 153),
			EnResourceKeyId.DefaultSelectedCommentAnchorBorderColor => Color.FromArgb(byte.MaxValue, 214, 168, 76),
			EnResourceKeyId.DefaultSelectedCommentAnchorFillColor => Color.FromArgb(byte.MaxValue, byte.MaxValue, 237, 153),
			EnResourceKeyId.DefaultUnselectedCommentAnchorBorderColor => Color.FromArgb(byte.MaxValue, 204, 153, 51),
			EnResourceKeyId.DefaultUnselectedCommentAnchorFillColor => Color.FromArgb(byte.MaxValue, byte.MaxValue, 237, 153),
			EnResourceKeyId.EmbeddedDialogBackgroundColor => SystemColors.ControlColor,
			EnResourceKeyId.CodeReviewDiscussionSelectedItemColor => Color.FromArgb(byte.MaxValue, 232, 240, 246),
			EnResourceKeyId.CodeReviewDiscussionSelectedItemColorTextColor => Color.FromArgb(byte.MaxValue, 0, 0, 0),
			EnResourceKeyId.CodeReviewDiscussionSelectedActionLinkColor => Color.FromArgb(byte.MaxValue, 0, 112, 192),
			EnResourceKeyId.CodeReviewDiscussionDisabledActionLinkColor => Color.FromArgb(byte.MaxValue, 113, 113, 113),
			EnResourceKeyId.ConnDlgSelectedTabUnderLineColor => Color.FromArgb(byte.MaxValue, 57, 156, 223),
			EnResourceKeyId.EmphasizedTextColor => SystemColors.WindowTextColor,
			EnResourceKeyId.IconActionFillColor => Color.FromArgb(byte.MaxValue, 0, 162, 232),
			EnResourceKeyId.IconGeneralFillColor => Color.FromArgb(byte.MaxValue, 65, 65, 65),
			EnResourceKeyId.IconGeneralStrokeColor => Color.FromArgb(byte.MaxValue, 217, 217, 217),
			EnResourceKeyId.InnerTabActiveBackgroundColor => SystemColors.ControlColor,
			EnResourceKeyId.InnerTabActiveTextColor => SystemColors.ControlTextColor,
			EnResourceKeyId.InnerTabHeaderBackgroundColor => SystemColors.WindowColor,
			EnResourceKeyId.InnerTabHeaderTextColor => SystemColors.WindowTextColor,
			EnResourceKeyId.InnerTabHoverBackgroundColor => SystemColors.ControlDarkColor,
			EnResourceKeyId.InnerTabHoverTextColor => SystemColors.ControlTextColor,
			EnResourceKeyId.InnerTabInactiveBackgroundColor => SystemColors.WindowColor,
			EnResourceKeyId.InnerTabInactiveTextColor => SystemColors.WindowTextColor,
			EnResourceKeyId.InnerTabPressedBackgroundColor => SystemColors.ControlColor,
			EnResourceKeyId.InnerTabPressedTextColor => SystemColors.ControlTextColor,
			EnResourceKeyId.ItemBorderColor => SystemColors.WindowColor,
			EnResourceKeyId.ItemColor => SystemColors.WindowColor,
			EnResourceKeyId.ItemHoverBorderColor => Color.FromArgb(byte.MaxValue, byte.MaxValue, 232, 166),
			EnResourceKeyId.ItemHoverColor => Color.FromArgb(byte.MaxValue, byte.MaxValue, 243, 206),
			EnResourceKeyId.ItemHoverTextColor => Color.FromArgb(byte.MaxValue, 0, 0, 0),
			EnResourceKeyId.ItemSelectedBorderColor => Color.FromArgb(byte.MaxValue, 201, 236, 250),
			EnResourceKeyId.ItemSelectedBorderNotFocusedColor => Color.FromArgb(byte.MaxValue, 240, 240, 240),
			EnResourceKeyId.ItemSelectedColor => Color.FromArgb(byte.MaxValue, 232, 240, 246),
			EnResourceKeyId.ItemSelectedNotFocusedColor => Color.FromArgb(byte.MaxValue, 240, 240, 240),
			EnResourceKeyId.ItemSelectedTextColor => Color.FromArgb(byte.MaxValue, 0, 0, 0),
			EnResourceKeyId.ItemSelectedTextNotFocusedColor => Color.FromArgb(byte.MaxValue, 0, 0, 0),
			EnResourceKeyId.ItemTextColor => SystemColors.WindowTextColor,
			EnResourceKeyId.MenuBackgroundColor => SystemColors.ControlColor,
			EnResourceKeyId.MenuBorderColor => SystemColors.ControlDarkColor,
			EnResourceKeyId.MenuHoverBackgroundColor => Color.FromArgb(byte.MaxValue, 232, 240, 246),
			EnResourceKeyId.MenuHoverTextColor => SystemColors.ControlTextColor,
			EnResourceKeyId.MenuSeparatorColor => Color.FromArgb(byte.MaxValue, 224, 224, 224),
			EnResourceKeyId.MenuTextColor => SystemColors.ControlTextColor,
			EnResourceKeyId.NotificationActionLinkColor => SystemColors.HotTrackColor,
			EnResourceKeyId.NotificationColor => SystemColors.WindowColor,
			EnResourceKeyId.NotificationTextColor => SystemColors.WindowTextColor,
			EnResourceKeyId.ProgressBarBackgroundColor => SystemColors.ControlColor,
			EnResourceKeyId.ProgressBarForegroundColor => SystemColors.ControlDarkColor,
			EnResourceKeyId.RequiredTextBoxBorderColor => SystemColors.WindowColor,
			EnResourceKeyId.ScrollBarBackgroundColor => SystemColors.ControlColor,
			EnResourceKeyId.SectionTitleTextColor => Color.FromArgb(byte.MaxValue, 31, 71, 125),
			EnResourceKeyId.SubduedBorderColor => SystemColors.GrayTextColor,
			EnResourceKeyId.SubduedTextColor => SystemColors.GrayTextColor,
			EnResourceKeyId.SubtitleTextColor => SystemColors.WindowTextColor,
			EnResourceKeyId.TextBoxBorderColor => SystemColors.WindowColor,
			EnResourceKeyId.TextBoxColor => SystemColors.WindowColor,
			EnResourceKeyId.TextBoxHintTextColor => SystemColors.GrayTextColor,
			EnResourceKeyId.TextBoxTextColor => SystemColors.WindowTextColor,
			EnResourceKeyId.TitleBarInactiveColor => SystemColors.ControlColor,
			EnResourceKeyId.TitleBarInactiveTextColor => SystemColors.ControlTextColor,
			EnResourceKeyId.TitleTextColor => Color.FromArgb(byte.MaxValue, 120, 148, 64),
			EnResourceKeyId.ToolbarColor => SystemColors.ControlColor,
			EnResourceKeyId.ToolbarSelectedBorderColor => SystemColors.ControlTextColor,
			EnResourceKeyId.ToolbarSelectedColor => SystemColors.ControlDarkColor,
			EnResourceKeyId.ToolbarSelectedTextColor => SystemColors.ControlTextColor,
			EnResourceKeyId.ToolbarTextColor => SystemColors.ControlTextColor,
			EnResourceKeyId.ToolWindowBackgroundColor => SystemColors.ControlColor,
			EnResourceKeyId.ToolWindowBorderColor => SystemColors.ControlColor,
			EnResourceKeyId.TeamExplorerHomeDefaultIconFillColor => Color.FromArgb(byte.MaxValue, 0, 121, 206),
			EnResourceKeyId.TeamExplorerHomeWITFamilyIconFillColor => Color.FromArgb(byte.MaxValue, 0, 158, 206),
			EnResourceKeyId.TeamExplorerHomeSCCFamilyIconFillColor => Color.FromArgb(byte.MaxValue, 104, 33, 122),
			EnResourceKeyId.TeamExplorerHomeBuildFamilyIconFillColor => Color.FromArgb(byte.MaxValue, 115, 130, 140),
			EnResourceKeyId.TeamExplorerHomeSCCGitFamilyIconFillColor => Color.FromArgb(byte.MaxValue, 240, 80, 51),
			EnResourceKeyId.TeamExplorerHomeDocumentsFamilyIconFillColor => Color.FromArgb(byte.MaxValue, 249, 201, 0),
			EnResourceKeyId.TeamExplorerHomeReportsFamilyIconFillColor => Color.FromArgb(byte.MaxValue, 174, 60, 186),
			EnResourceKeyId.TETileIconBackgroundColor => Color.FromArgb(byte.MaxValue, 228, 228, 231),
			EnResourceKeyId.TETileIconBackgroundMouseOverColor => Color.FromArgb(byte.MaxValue, 181, 202, 231),
			EnResourceKeyId.TETileIconBackgroundSelectedColor => Color.FromArgb(byte.MaxValue, 137, 188, 241),
			EnResourceKeyId.TETileListBackgroundColor => Color.FromArgb(byte.MaxValue, 238, 238, 241),
			EnResourceKeyId.TETileListBackgroundMouseOverColor => Color.FromArgb(byte.MaxValue, 201, 222, 245),
			EnResourceKeyId.TETileListBackgroundSelectedColor => Color.FromArgb(byte.MaxValue, 51, 153, byte.MaxValue),
			EnResourceKeyId.TETileListForegroundColor => Color.FromArgb(byte.MaxValue, 30, 30, 30),
			EnResourceKeyId.TETileListForegroundMouseOverColor => Color.FromArgb(byte.MaxValue, 30, 30, 30),
			EnResourceKeyId.TETileListForegroundSelectedColor => Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue),
			EnResourceKeyId.TETileBorderColor => Color.FromArgb(0, 0, 0, 0),
			EnResourceKeyId.VSLogoIconFillColor => Color.FromArgb(byte.MaxValue, 65, 65, 65),
			EnResourceKeyId.WorkItemActiveControlBackground => SystemColors.HighlightColor,
			EnResourceKeyId.WorkItemActiveControlForeground => SystemColors.HighlightTextColor,
			EnResourceKeyId.WorkItemActiveControlBorder => SystemColors.HighlightColor,
			EnResourceKeyId.WorkItemButtonSelectedOverride => SystemColors.HighlightColor,
			EnResourceKeyId.WorkItemDefaultControlBackground => SystemColors.WindowColor,
			EnResourceKeyId.WorkItemDefaultControlForeground => SystemColors.WindowTextColor,
			EnResourceKeyId.WorkItemDefaultControlBorder => SystemColors.ControlDarkColor,
			EnResourceKeyId.WorkItemDropDownHoverBackground => SystemColors.ControlColor,
			EnResourceKeyId.WorkItemDropDownHoverForeground => SystemColors.HighlightColor,
			EnResourceKeyId.WorkItemDropDownPressedBackground => SystemColors.HighlightColor,
			EnResourceKeyId.WorkItemDropDownPressedForeground => SystemColors.HighlightTextColor,
			EnResourceKeyId.WorkItemDropDownInactiveBackground => SystemColors.HighlightColor,
			EnResourceKeyId.WorkItemDropDownInactiveForeground => SystemColors.HighlightTextColor,
			EnResourceKeyId.WorkItemErrorControlBackground => SystemColors.WindowColor,
			EnResourceKeyId.WorkItemErrorControlForeground => SystemColors.GrayTextColor,
			EnResourceKeyId.WorkItemErrorControlBorder => Color.FromArgb(byte.MaxValue, byte.MaxValue, 0, 0),
			EnResourceKeyId.WorkItemFormBackground => SystemColors.WindowColor,
			EnResourceKeyId.WorkItemFormForeground => SystemColors.WindowTextColor,
			EnResourceKeyId.WorkItemHtmlControlBackground => SystemColors.WindowColor,
			EnResourceKeyId.WorkItemHtmlControlForeground => SystemColors.WindowTextColor,
			EnResourceKeyId.WorkItemHtmlControlHyperlink => SystemColors.HighlightColor,
			EnResourceKeyId.WorkItemHtmlControlSecondaryText => SystemColors.GrayTextColor,
			EnResourceKeyId.WorkItemHtmlScrollbarArrow => Color.FromArgb(byte.MaxValue, 96, 96, 96),
			EnResourceKeyId.WorkItemHtmlScrollbarTrack => Color.FromArgb(byte.MaxValue, 239, 239, 239),
			EnResourceKeyId.WorkItemHtmlScrollbarThumb => Color.FromArgb(byte.MaxValue, 204, 204, 204),
			EnResourceKeyId.WorkItemInvalidControlBackground => SystemColors.InfoColor,
			EnResourceKeyId.WorkItemInvalidControlForeground => SystemColors.InfoTextColor,
			EnResourceKeyId.WorkItemInvalidControlBorder => SystemColors.ControlDarkColor,
			EnResourceKeyId.WorkItemGroupBoxBackground => SystemColors.WindowColor,
			EnResourceKeyId.WorkItemGroupBoxForeground => SystemColors.WindowTextColor,
			EnResourceKeyId.WorkItemLabelBackground => SystemColors.WindowColor,
			EnResourceKeyId.WorkItemLabelForeground => SystemColors.GrayTextColor,
			EnResourceKeyId.WorkItemPromptText => SystemColors.WindowTextColor,
			EnResourceKeyId.WorkItemReadOnlyControlBackground => SystemColors.WindowColor,
			EnResourceKeyId.WorkItemReadOnlyControlForeground => SystemColors.GrayTextColor,
			EnResourceKeyId.WorkItemReadOnlyControlBorder => SystemColors.WindowColor,
			EnResourceKeyId.WorkItemTabItemBackground => SystemColors.WindowColor,
			EnResourceKeyId.WorkItemTabItemForeground => SystemColors.WindowTextColor,
			EnResourceKeyId.WorkItemTabHeaderBackground => SystemColors.ControlColor,
			EnResourceKeyId.WorkItemTabHeaderForeground => SystemColors.ControlDarkDarkColor,
			EnResourceKeyId.WorkItemTabHeaderActiveForeground => SystemColors.WindowTextColor,
			EnResourceKeyId.WorkItemTabHeaderHoverForeground => Cmd.CombineColors(DialogColors.Instance.WorkItemTabHeaderBackground, 50, DialogColors.Instance.WorkItemTabHeaderForeground, 50),
			EnResourceKeyId.WorkItemGridBackground => SystemColors.WindowColor,
			EnResourceKeyId.WorkItemGridForeground => SystemColors.WindowTextColor,
			EnResourceKeyId.WorkItemGridBorder => SystemColors.ControlLightColor,
			EnResourceKeyId.WorkItemGridRowHeaderBackground => SystemColors.ControlColor,
			EnResourceKeyId.WorkItemGridRowHeaderForeground => SystemColors.ControlTextColor,
			EnResourceKeyId.WorkItemGridColumnHeaderBackground => SystemColors.ControlColor,
			EnResourceKeyId.WorkItemGridColumnHeaderForeground => SystemColors.ControlTextColor,
			EnResourceKeyId.WorkItemGridColumnHeaderHoverBackground => SystemColors.HighlightColor,
			EnResourceKeyId.WorkItemGridColumnHeaderHoverForeground => SystemColors.HighlightTextColor,
			EnResourceKeyId.WorkItemGridColumnHeaderPressedBackground => SystemColors.HighlightColor,
			EnResourceKeyId.WorkItemGridColumnHeaderPressedForeground => SystemColors.HighlightTextColor,
			EnResourceKeyId.WorkItemGridActiveRowHeaderBackground => SystemColors.HighlightColor,
			EnResourceKeyId.WorkItemGridActiveRowHeaderForeground => SystemColors.HighlightTextColor,
			EnResourceKeyId.WorkItemGridActiveColumnHeaderBackground => SystemColors.HighlightColor,
			EnResourceKeyId.WorkItemGridActiveColumnHeaderForeground => SystemColors.HighlightTextColor,
			EnResourceKeyId.WorkItemGridCellBackground => SystemColors.WindowColor,
			EnResourceKeyId.WorkItemGridCellForeground => SystemColors.WindowTextColor,
			EnResourceKeyId.WorkItemGridActiveCellBackground => SystemColors.HighlightColor,
			EnResourceKeyId.WorkItemGridActiveCellForeground => SystemColors.HighlightTextColor,
			EnResourceKeyId.WorkItemGridInactiveCellBackground => SystemColors.ControlColor,
			EnResourceKeyId.WorkItemGridInactiveCellForeground => SystemColors.ControlTextColor,
			EnResourceKeyId.WorkItemGridSortedCellBackground => SystemColors.ControlColor,
			EnResourceKeyId.WorkItemGridSortedCellForeground => SystemColors.ControlTextColor,
			EnResourceKeyId.WorkItemTrackingInfobarBackground => SystemColors.ControlColor,
			EnResourceKeyId.WorkItemTagForeground => Color.FromArgb(byte.MaxValue, 30, 30, 30),
			EnResourceKeyId.WorkItemTagBackground => Color.FromArgb(byte.MaxValue, 225, 230, 241),
			EnResourceKeyId.WorkItemTagActiveForeground => Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue),
			EnResourceKeyId.WorkItemTagActiveBackground => Color.FromArgb(byte.MaxValue, 51, 153, byte.MaxValue),
			EnResourceKeyId.WorkItemTagHoverForeground => Color.FromArgb(byte.MaxValue, 30, 30, 30),
			EnResourceKeyId.WorkItemTagHoverBackground => Color.FromArgb(byte.MaxValue, 201, 222, 245),
			EnResourceKeyId.WorkItemTagActiveGlyphHoverForeground => Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue),
			EnResourceKeyId.WorkItemTagActiveGlyphHoverBackground => Color.FromArgb(byte.MaxValue, 82, 176, 239),
			EnResourceKeyId.WorkItemTagActiveGlyphHoverBorder => Color.FromArgb(byte.MaxValue, 82, 176, byte.MaxValue),
			EnResourceKeyId.WorkItemTagActiveGlyphPressedForeground => Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue),
			EnResourceKeyId.WorkItemTagActiveGlyphPressedBackground => Color.FromArgb(byte.MaxValue, 14, 97, 152),
			EnResourceKeyId.WorkItemTagActiveGlyphPressedBorder => Color.FromArgb(byte.MaxValue, 14, 97, 152),
			EnResourceKeyId.WorkItemTagHoverGlyphHoverForeground => Color.FromArgb(byte.MaxValue, 30, 30, 30),
			EnResourceKeyId.WorkItemTagHoverGlyphHoverBackground => Color.FromArgb(byte.MaxValue, 247, 247, 249),
			EnResourceKeyId.WorkItemTagHoverGlyphHoverBorder => Color.FromArgb(byte.MaxValue, 247, 247, 249),
			EnResourceKeyId.WorkItemTagHoverGlyphPressedForeground => Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue),
			EnResourceKeyId.WorkItemTagHoverGlyphPressedBackground => Color.FromArgb(byte.MaxValue, 14, 97, 152),
			EnResourceKeyId.WorkItemTagHoverGlyphPressedBorder => Color.FromArgb(byte.MaxValue, 14, 97, 152),
			EnResourceKeyId.VersionControlAnnotateRegionBackgroundColor => SystemColors.ControlColor,
			EnResourceKeyId.VersionControlAnnotateRegionForegroundColor => SystemColors.ControlTextColor,
			EnResourceKeyId.VersionControlAnnotateRegionSelectedBackgroundColor => SystemColors.HighlightColor,
			EnResourceKeyId.VersionControlAnnotateRegionSelectedForegroundColor => SystemColors.HighlightTextColor,
			_ => Color.FromArgb(byte.MaxValue, 0, 0, 0),
		};
	}

	public virtual Color GetHighContrastColor(EnResourceKeyId id)
	{
		return id switch
		{
			EnResourceKeyId.ActionLinkColor => SystemColors.HotTrackColor,
			EnResourceKeyId.ActionLinkDisabledColor => SystemColors.GrayTextColor,
			EnResourceKeyId.ActionLinkItemColor => SystemColors.WindowTextColor,
			EnResourceKeyId.ActionLinkItemDisabledColor => SystemColors.GrayTextColor,
			EnResourceKeyId.ActionLinkItemHoverColor => SystemColors.HighlightTextColor,
			EnResourceKeyId.ActionLinkItemSelectedColor => SystemColors.HighlightTextColor,
			EnResourceKeyId.ActionLinkItemSelectedNotFocusedColor => SystemColors.HighlightTextColor,
			EnResourceKeyId.ArrowGlyphColor => SystemColors.WindowTextColor,
			EnResourceKeyId.ArrowGlyphMouseOverColor => SystemColors.HotTrackColor,
			EnResourceKeyId.BodyTextColor => SystemColors.WindowTextColor,
			EnResourceKeyId.ButtonBackgroundColor => SystemColors.ControlColor,
			EnResourceKeyId.ButtonBorderColor => SystemColors.ControlColor,
			EnResourceKeyId.ButtonDisabledBackgroundColor => SystemColors.ControlColor,
			EnResourceKeyId.ButtonDisabledBorderColor => SystemColors.ControlColor,
			EnResourceKeyId.ButtonDisabledForegroundColor => SystemColors.ControlTextColor,
			EnResourceKeyId.ButtonForegroundColor => SystemColors.ControlTextColor,
			EnResourceKeyId.ButtonHoverBackgroundColor => SystemColors.ControlColor,
			EnResourceKeyId.ButtonHoverBorderColor => SystemColors.ControlColor,
			EnResourceKeyId.ButtonHoverForegroundColor => SystemColors.ControlTextColor,
			EnResourceKeyId.ButtonPressedBackgroundColor => SystemColors.ControlColor,
			EnResourceKeyId.ButtonPressedBorderColor => SystemColors.ControlColor,
			EnResourceKeyId.ButtonPressedForegroundColor => SystemColors.ControlTextColor,
			EnResourceKeyId.DefaultSelectedAnchorPointBorderColor => SystemColors.ActiveBorderColor,
			EnResourceKeyId.DefaultSelectedAnchorPointColor => SystemColors.ActiveCaptionColor,
			EnResourceKeyId.DefaultUnselectedAnchorPointBorderColor => SystemColors.InactiveBorderColor,
			EnResourceKeyId.DefaultUnselectedAnchorPointColor => SystemColors.InactiveCaptionColor,
			EnResourceKeyId.DefaultSelectedCommentAnchorBorderColor => Color.FromArgb(byte.MaxValue, 211, 170, 61),
			EnResourceKeyId.DefaultSelectedCommentAnchorFillColor => Color.FromArgb(byte.MaxValue, 140, 97, 11),
			EnResourceKeyId.DefaultUnselectedCommentAnchorBorderColor => Color.FromArgb(byte.MaxValue, 230, 195, 102),
			EnResourceKeyId.DefaultUnselectedCommentAnchorFillColor => Color.FromArgb(byte.MaxValue, 140, 97, 11),
			EnResourceKeyId.EmbeddedDialogBackgroundColor => SystemColors.ControlColor,
			EnResourceKeyId.CodeReviewDiscussionSelectedItemColor => SystemColors.HighlightColor,
			EnResourceKeyId.CodeReviewDiscussionSelectedItemColorTextColor => SystemColors.HighlightTextColor,
			EnResourceKeyId.CodeReviewDiscussionSelectedActionLinkColor => SystemColors.HighlightTextColor,
			EnResourceKeyId.CodeReviewDiscussionDisabledActionLinkColor => SystemColors.HighlightColor,
			EnResourceKeyId.ConnDlgSelectedTabUnderLineColor => SystemColors.HighlightColor,
			EnResourceKeyId.EmphasizedTextColor => SystemColors.WindowTextColor,
			EnResourceKeyId.IconActionFillColor => Color.FromArgb(byte.MaxValue, 0, 162, 232),
			EnResourceKeyId.IconGeneralFillColor => Color.FromArgb(byte.MaxValue, 65, 65, 65),
			EnResourceKeyId.IconGeneralStrokeColor => Color.FromArgb(byte.MaxValue, 217, 217, 217),
			EnResourceKeyId.InnerTabActiveBackgroundColor => SystemColors.ControlColor,
			EnResourceKeyId.InnerTabActiveTextColor => SystemColors.ControlTextColor,
			EnResourceKeyId.InnerTabHeaderBackgroundColor => SystemColors.WindowColor,
			EnResourceKeyId.InnerTabHeaderTextColor => SystemColors.WindowTextColor,
			EnResourceKeyId.InnerTabHoverBackgroundColor => SystemColors.ControlDarkColor,
			EnResourceKeyId.InnerTabHoverTextColor => SystemColors.ControlTextColor,
			EnResourceKeyId.InnerTabInactiveBackgroundColor => SystemColors.WindowColor,
			EnResourceKeyId.InnerTabInactiveTextColor => SystemColors.WindowTextColor,
			EnResourceKeyId.InnerTabPressedBackgroundColor => SystemColors.ControlColor,
			EnResourceKeyId.InnerTabPressedTextColor => SystemColors.ControlTextColor,
			EnResourceKeyId.ItemBorderColor => SystemColors.WindowColor,
			EnResourceKeyId.ItemColor => SystemColors.WindowColor,
			EnResourceKeyId.ItemHoverBorderColor => SystemColors.HighlightColor,
			EnResourceKeyId.ItemHoverColor => SystemColors.HighlightColor,
			EnResourceKeyId.ItemHoverTextColor => SystemColors.HighlightTextColor,
			EnResourceKeyId.ItemSelectedBorderColor => SystemColors.HighlightColor,
			EnResourceKeyId.ItemSelectedBorderNotFocusedColor => SystemColors.HighlightColor,
			EnResourceKeyId.ItemSelectedColor => SystemColors.HighlightColor,
			EnResourceKeyId.ItemSelectedNotFocusedColor => SystemColors.HighlightColor,
			EnResourceKeyId.ItemSelectedTextColor => SystemColors.HighlightTextColor,
			EnResourceKeyId.ItemSelectedTextNotFocusedColor => SystemColors.HighlightTextColor,
			EnResourceKeyId.ItemTextColor => SystemColors.WindowTextColor,
			EnResourceKeyId.MenuBackgroundColor => SystemColors.ControlColor,
			EnResourceKeyId.MenuBorderColor => SystemColors.ControlDarkColor,
			EnResourceKeyId.MenuHoverBackgroundColor => SystemColors.HighlightColor,
			EnResourceKeyId.MenuHoverTextColor => SystemColors.ControlTextColor,
			EnResourceKeyId.MenuSeparatorColor => SystemColors.ControlDarkColor,
			EnResourceKeyId.MenuTextColor => SystemColors.ControlTextColor,
			EnResourceKeyId.NotificationActionLinkColor => SystemColors.HotTrackColor,
			EnResourceKeyId.NotificationColor => SystemColors.WindowColor,
			EnResourceKeyId.NotificationTextColor => SystemColors.WindowTextColor,
			EnResourceKeyId.ProgressBarBackgroundColor => SystemColors.ControlColor,
			EnResourceKeyId.ProgressBarForegroundColor => SystemColors.ControlDarkColor,
			EnResourceKeyId.RequiredTextBoxBorderColor => SystemColors.WindowFrameColor,
			EnResourceKeyId.ScrollBarBackgroundColor => SystemColors.ControlColor,
			EnResourceKeyId.SectionTitleTextColor => SystemColors.WindowTextColor,
			EnResourceKeyId.SubduedBorderColor => SystemColors.GrayTextColor,
			EnResourceKeyId.SubduedTextColor => SystemColors.GrayTextColor,
			EnResourceKeyId.SubtitleTextColor => SystemColors.WindowTextColor,
			EnResourceKeyId.TextBoxBorderColor => SystemColors.WindowFrameColor,
			EnResourceKeyId.TextBoxColor => SystemColors.WindowColor,
			EnResourceKeyId.TextBoxHintTextColor => SystemColors.GrayTextColor,
			EnResourceKeyId.TextBoxTextColor => SystemColors.WindowTextColor,
			EnResourceKeyId.TitleBarInactiveColor => SystemColors.ControlColor,
			EnResourceKeyId.TitleBarInactiveTextColor => SystemColors.ControlTextColor,
			EnResourceKeyId.TitleTextColor => SystemColors.WindowTextColor,
			EnResourceKeyId.ToolbarColor => SystemColors.ControlColor,
			EnResourceKeyId.ToolbarSelectedBorderColor => SystemColors.ControlTextColor,
			EnResourceKeyId.ToolbarSelectedColor => SystemColors.ControlDarkColor,
			EnResourceKeyId.ToolbarSelectedTextColor => SystemColors.ControlTextColor,
			EnResourceKeyId.ToolbarTextColor => SystemColors.ControlTextColor,
			EnResourceKeyId.ToolWindowBackgroundColor => SystemColors.ControlColor,
			EnResourceKeyId.ToolWindowBorderColor => SystemColors.ControlColor,
			EnResourceKeyId.TeamExplorerHomeDefaultIconFillColor => SystemColors.ControlColor,
			EnResourceKeyId.TeamExplorerHomeWITFamilyIconFillColor => SystemColors.ControlColor,
			EnResourceKeyId.TeamExplorerHomeSCCFamilyIconFillColor => SystemColors.ControlColor,
			EnResourceKeyId.TeamExplorerHomeBuildFamilyIconFillColor => SystemColors.ControlColor,
			EnResourceKeyId.TeamExplorerHomeSCCGitFamilyIconFillColor => SystemColors.ControlColor,
			EnResourceKeyId.TeamExplorerHomeDocumentsFamilyIconFillColor => SystemColors.ControlColor,
			EnResourceKeyId.TeamExplorerHomeReportsFamilyIconFillColor => SystemColors.ControlColor,
			EnResourceKeyId.TETileIconBackgroundColor => SystemColors.ControlLightLightColor,
			EnResourceKeyId.TETileIconBackgroundMouseOverColor => SystemColors.HighlightColor,
			EnResourceKeyId.TETileIconBackgroundSelectedColor => SystemColors.ActiveCaptionColor,
			EnResourceKeyId.TETileListBackgroundColor => SystemColors.ControlDarkColor,
			EnResourceKeyId.TETileListBackgroundMouseOverColor => SystemColors.HighlightColor,
			EnResourceKeyId.TETileListBackgroundSelectedColor => SystemColors.ActiveCaptionColor,
			EnResourceKeyId.TETileListForegroundColor => SystemColors.ControlTextColor,
			EnResourceKeyId.TETileListForegroundMouseOverColor => SystemColors.ActiveCaptionTextColor,
			EnResourceKeyId.TETileListForegroundSelectedColor => SystemColors.HighlightTextColor,
			EnResourceKeyId.TETileBorderColor => SystemColors.ControlDarkColor,
			EnResourceKeyId.VSLogoIconFillColor => SystemColors.WindowTextColor,
			EnResourceKeyId.WorkItemActiveControlBackground => SystemColors.HighlightColor,
			EnResourceKeyId.WorkItemActiveControlForeground => SystemColors.HighlightTextColor,
			EnResourceKeyId.WorkItemActiveControlBorder => SystemColors.HighlightColor,
			EnResourceKeyId.WorkItemButtonSelectedOverride => SystemColors.HighlightColor,
			EnResourceKeyId.WorkItemDefaultControlBackground => SystemColors.WindowColor,
			EnResourceKeyId.WorkItemDefaultControlForeground => SystemColors.WindowTextColor,
			EnResourceKeyId.WorkItemDefaultControlBorder => SystemColors.ControlDarkColor,
			EnResourceKeyId.WorkItemDropDownHoverBackground => SystemColors.ControlColor,
			EnResourceKeyId.WorkItemDropDownHoverForeground => SystemColors.HighlightColor,
			EnResourceKeyId.WorkItemDropDownPressedBackground => SystemColors.HighlightColor,
			EnResourceKeyId.WorkItemDropDownPressedForeground => SystemColors.HighlightTextColor,
			EnResourceKeyId.WorkItemDropDownInactiveBackground => SystemColors.HighlightColor,
			EnResourceKeyId.WorkItemDropDownInactiveForeground => SystemColors.HighlightTextColor,
			EnResourceKeyId.WorkItemErrorControlBackground => SystemColors.WindowColor,
			EnResourceKeyId.WorkItemErrorControlForeground => SystemColors.GrayTextColor,
			EnResourceKeyId.WorkItemErrorControlBorder => SystemColors.WindowColor,
			EnResourceKeyId.WorkItemFormBackground => SystemColors.WindowColor,
			EnResourceKeyId.WorkItemFormForeground => SystemColors.WindowTextColor,
			EnResourceKeyId.WorkItemHtmlControlBackground => SystemColors.WindowColor,
			EnResourceKeyId.WorkItemHtmlControlForeground => SystemColors.WindowTextColor,
			EnResourceKeyId.WorkItemHtmlControlHyperlink => SystemColors.HighlightColor,
			EnResourceKeyId.WorkItemHtmlControlSecondaryText => SystemColors.GrayTextColor,
			EnResourceKeyId.WorkItemHtmlScrollbarArrow => SystemColors.WindowTextColor,
			EnResourceKeyId.WorkItemHtmlScrollbarTrack => SystemColors.ScrollBarColor,
			EnResourceKeyId.WorkItemHtmlScrollbarThumb => SystemColors.ControlDarkColor,
			EnResourceKeyId.WorkItemInvalidControlBackground => SystemColors.InfoColor,
			EnResourceKeyId.WorkItemInvalidControlForeground => SystemColors.InfoTextColor,
			EnResourceKeyId.WorkItemInvalidControlBorder => SystemColors.ControlDarkColor,
			EnResourceKeyId.WorkItemGroupBoxBackground => SystemColors.WindowColor,
			EnResourceKeyId.WorkItemGroupBoxForeground => SystemColors.WindowTextColor,
			EnResourceKeyId.WorkItemLabelBackground => SystemColors.WindowColor,
			EnResourceKeyId.WorkItemLabelForeground => SystemColors.GrayTextColor,
			EnResourceKeyId.WorkItemPromptText => SystemColors.WindowTextColor,
			EnResourceKeyId.WorkItemReadOnlyControlBackground => SystemColors.WindowColor,
			EnResourceKeyId.WorkItemReadOnlyControlForeground => SystemColors.GrayTextColor,
			EnResourceKeyId.WorkItemReadOnlyControlBorder => SystemColors.WindowColor,
			EnResourceKeyId.WorkItemTabItemBackground => SystemColors.WindowColor,
			EnResourceKeyId.WorkItemTabItemForeground => SystemColors.WindowTextColor,
			EnResourceKeyId.WorkItemTabHeaderBackground => SystemColors.ControlColor,
			EnResourceKeyId.WorkItemTabHeaderForeground => SystemColors.ControlDarkDarkColor,
			EnResourceKeyId.WorkItemTabHeaderActiveForeground => SystemColors.WindowTextColor,
			EnResourceKeyId.WorkItemTabHeaderHoverForeground => Cmd.CombineColors(DialogColors.Instance.WorkItemTabHeaderBackground, 50, DialogColors.Instance.WorkItemTabHeaderForeground, 50),
			EnResourceKeyId.WorkItemGridBackground => SystemColors.WindowColor,
			EnResourceKeyId.WorkItemGridForeground => SystemColors.WindowTextColor,
			EnResourceKeyId.WorkItemGridBorder => SystemColors.ControlLightColor,
			EnResourceKeyId.WorkItemGridRowHeaderBackground => SystemColors.ControlColor,
			EnResourceKeyId.WorkItemGridRowHeaderForeground => SystemColors.ControlTextColor,
			EnResourceKeyId.WorkItemGridColumnHeaderBackground => SystemColors.ControlColor,
			EnResourceKeyId.WorkItemGridColumnHeaderForeground => SystemColors.ControlTextColor,
			EnResourceKeyId.WorkItemGridColumnHeaderHoverBackground => SystemColors.HighlightColor,
			EnResourceKeyId.WorkItemGridColumnHeaderHoverForeground => SystemColors.HighlightTextColor,
			EnResourceKeyId.WorkItemGridColumnHeaderPressedBackground => SystemColors.HighlightColor,
			EnResourceKeyId.WorkItemGridColumnHeaderPressedForeground => SystemColors.HighlightTextColor,
			EnResourceKeyId.WorkItemGridActiveRowHeaderBackground => SystemColors.HighlightColor,
			EnResourceKeyId.WorkItemGridActiveRowHeaderForeground => SystemColors.HighlightTextColor,
			EnResourceKeyId.WorkItemGridActiveColumnHeaderBackground => SystemColors.HighlightColor,
			EnResourceKeyId.WorkItemGridActiveColumnHeaderForeground => SystemColors.HighlightTextColor,
			EnResourceKeyId.WorkItemGridCellBackground => SystemColors.WindowColor,
			EnResourceKeyId.WorkItemGridCellForeground => SystemColors.WindowTextColor,
			EnResourceKeyId.WorkItemGridActiveCellBackground => SystemColors.HighlightColor,
			EnResourceKeyId.WorkItemGridActiveCellForeground => SystemColors.HighlightTextColor,
			EnResourceKeyId.WorkItemGridInactiveCellBackground => SystemColors.ControlColor,
			EnResourceKeyId.WorkItemGridInactiveCellForeground => SystemColors.ControlTextColor,
			EnResourceKeyId.WorkItemGridSortedCellBackground => SystemColors.ControlColor,
			EnResourceKeyId.WorkItemGridSortedCellForeground => SystemColors.ControlTextColor,
			EnResourceKeyId.WorkItemTrackingInfobarBackground => SystemColors.ControlColor,
			EnResourceKeyId.WorkItemTagForeground => SystemColors.ActiveCaptionTextColor,
			EnResourceKeyId.WorkItemTagBackground => SystemColors.ActiveCaptionColor,
			EnResourceKeyId.WorkItemTagActiveForeground => SystemColors.HighlightTextColor,
			EnResourceKeyId.WorkItemTagActiveBackground => SystemColors.HighlightColor,
			EnResourceKeyId.WorkItemTagHoverForeground => SystemColors.HighlightTextColor,
			EnResourceKeyId.WorkItemTagHoverBackground => SystemColors.HighlightColor,
			EnResourceKeyId.WorkItemTagActiveGlyphHoverForeground => SystemColors.HighlightColor,
			EnResourceKeyId.WorkItemTagActiveGlyphHoverBackground => SystemColors.HighlightTextColor,
			EnResourceKeyId.WorkItemTagActiveGlyphHoverBorder => SystemColors.HighlightTextColor,
			EnResourceKeyId.WorkItemTagActiveGlyphPressedForeground => SystemColors.HighlightColor,
			EnResourceKeyId.WorkItemTagActiveGlyphPressedBackground => SystemColors.HighlightTextColor,
			EnResourceKeyId.WorkItemTagActiveGlyphPressedBorder => SystemColors.HighlightTextColor,
			EnResourceKeyId.WorkItemTagHoverGlyphHoverForeground => SystemColors.HighlightColor,
			EnResourceKeyId.WorkItemTagHoverGlyphHoverBackground => SystemColors.HighlightTextColor,
			EnResourceKeyId.WorkItemTagHoverGlyphHoverBorder => SystemColors.HighlightTextColor,
			EnResourceKeyId.WorkItemTagHoverGlyphPressedForeground => SystemColors.HighlightColor,
			EnResourceKeyId.WorkItemTagHoverGlyphPressedBackground => SystemColors.HighlightTextColor,
			EnResourceKeyId.WorkItemTagHoverGlyphPressedBorder => SystemColors.HighlightTextColor,
			EnResourceKeyId.VersionControlAnnotateRegionBackgroundColor => SystemColors.ControlColor,
			EnResourceKeyId.VersionControlAnnotateRegionForegroundColor => SystemColors.ControlTextColor,
			EnResourceKeyId.VersionControlAnnotateRegionSelectedBackgroundColor => SystemColors.HighlightColor,
			EnResourceKeyId.VersionControlAnnotateRegionSelectedForegroundColor => SystemColors.HighlightTextColor,
			_ => SystemColors.ControlTextColor,
		};
	}
}
