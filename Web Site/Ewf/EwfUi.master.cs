﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using EnterpriseWebLibrary.Configuration;
using EnterpriseWebLibrary.EnterpriseWebFramework.Controls;
using EnterpriseWebLibrary.EnterpriseWebFramework.Ui;
using EnterpriseWebLibrary.EnterpriseWebFramework.Ui.Entity;
using EnterpriseWebLibrary.WebSessionState;
using Humanizer;

namespace EnterpriseWebLibrary.EnterpriseWebFramework.EnterpriseWebLibrary.WebSite {
	public partial class EwfUi: MasterPage, ControlTreeDataLoader, AppEwfUiMasterPage {
		internal class CssElementCreator: ControlCssElementCreator {
			internal const string GlobalBlockId = "ewfUiGlobal";
			internal const string AppLogoAndUserInfoBlockCssClass = "ewfUiAppLogoAndUserInfo";
			internal static readonly ElementClass AppLogoClass = new ElementClass( "ewfUiAppLogo" );
			internal const string UserInfoListCssClass = "ewfUiUserInfo";
			internal const string GlobalNavBlockCssClass = "ewfUiGlobalNav";

			internal static readonly ElementClass TopErrorMessageListContainerClass = new ElementClass( "ewfUiStatus" );

			internal const string EntityAndTabAndContentBlockId = "ewfUiEntityAndTabsAndContent";

			internal const string EntityAndTopTabBlockCssClass = "ewfUiEntityAndTopTabs";

			internal const string EntityBlockCssClass = "ewfUiEntity";
			internal const string EntityNavAndActionBlockCssClass = "ewfUiEntityNavAndActions";
			internal const string EntityNavListCssClass = "ewfUiEntityNav";
			internal const string EntityActionListCssClass = "ewfUiEntityActions";
			internal const string EntitySummaryBlockCssClass = "ewfUiEntitySummary";

			internal const string TopTabCssClass = "ewfUiTopTab";

			internal const string SideTabAndContentBlockCssClass = "ewfUiTabsAndContent";

			internal const string SideTabCssClass = "ewfUiSideTab";
			internal static readonly ElementClass SideTabGroupHeadClass = new ElementClass( "ewfEditorTabSeparator" );

			internal const string CurrentTabCssClass = "ewfEditorSelectedTab";
			internal const string DisabledTabCssClass = "ewfUiDisabledTab";

			internal const string ContentCssClass = "ewfUiContent";
			internal const string ContentFootBlockCssClass = "ewfButtons";
			internal const string ContentFootActionListCssClass = "ewfUiCfActions";

			internal const string GlobalFootBlockId = "ewfUiGlobalFoot";
			internal static readonly ElementClass PoweredByEwlFooterClass = new ElementClass( "ewfUiPoweredBy" );


			// Some of the elements below cover a subset of other CSS elements in a more specific way. For example, UiGlobalNavControlList selects the control list
			// used for global navigation. This control list is also selected, with lower specificity, by the CSS element that selects all control lists. In general
			// this is a bad situation, but in this case we think it's ok because web apps are not permitted to add their own CSS classes to the controls selected
			// here and therefore it will be difficult for a web app to accidentally trump a CSS element here by adding classes to a lower-specificity element. It
			// would still be possible to accidentally trump some of the rules in the EWF UI style sheets by chaining together several lower-specificity elements, but
			// we protect against this by incorporating an ID into the selectors here. A selector with an ID should always trump a selector without any IDs.

			IReadOnlyCollection<CssElement> ControlCssElementCreator.CreateCssElements() {
				return getGlobalElements().Concat( getEntityAndTabAndContentElements() ).Concat( getGlobalFootElements() ).ToArray();
			}

			private IEnumerable<CssElement> getGlobalElements() {
				const string globalBlockSelector = "div#" + GlobalBlockId;
				const string globalNavBlockSelector = globalBlockSelector + " " + "div." + GlobalNavBlockCssClass;
				return new[]
					{
						new CssElement( "UiGlobalBlock", globalBlockSelector ),
						new CssElement(
							"UiAppLogoAndUserInfoBlock",
							EwfTable.CssElementCreator.Selectors.Select( i => globalBlockSelector + " " + i + "." + AppLogoAndUserInfoBlockCssClass ).ToArray() ),
						new CssElement( "UiAppLogoContainer", globalBlockSelector + " " + "div." + AppLogoClass.ClassName ),
						new CssElement(
							"UiUserInfoControlList",
							ControlStack.CssElementCreator.Selectors.Select( i => globalBlockSelector + " " + i + "." + UserInfoListCssClass ).ToArray() ),
						new CssElement( "UiGlobalNavBlock", globalNavBlockSelector ),
						new CssElement( "UiGlobalNavControlList", ControlLine.CssElementCreator.Selectors.Select( i => globalNavBlockSelector + " > " + i ).ToArray() ),
						new CssElement(
							"UiTopErrorMessageListContainer",
							ListErrorDisplayStyle.CssSelectors.Select( i => globalBlockSelector + " " + i + "." + TopErrorMessageListContainerClass.ClassName ).ToArray() )
					};
			}

			private IEnumerable<CssElement> getEntityAndTabAndContentElements() {
				var elements = new List<CssElement>();

				const string entityAndTabAndContentBlockSelector = "div#" + EntityAndTabAndContentBlockId;
				elements.Add( new CssElement( "UiEntityAndTabAndContentBlock", entityAndTabAndContentBlockSelector ) );

				elements.Add( new CssElement( "UiEntityAndTopTabBlock", entityAndTabAndContentBlockSelector + " > " + "div." + EntityAndTopTabBlockCssClass ) );
				elements.AddRange( getEntityElements( entityAndTabAndContentBlockSelector ) );
				elements.Add( new CssElement( "UiTopTabBlock", entityAndTabAndContentBlockSelector + " " + "div." + TopTabCssClass ) );
				elements.AddRange( getSideTabAndContentElements( entityAndTabAndContentBlockSelector ) );
				elements.AddRange( getTabElements() );
				return elements;
			}

			private IEnumerable<CssElement> getEntityElements( string entityAndTabAndContentBlockSelector ) {
				return new[]
					{
						new CssElement( "UiEntityBlock", entityAndTabAndContentBlockSelector + " " + "div." + EntityBlockCssClass ),
						new CssElement(
							"UiEntityNavAndActionBlock",
							EwfTable.CssElementCreator.Selectors.Select( i => entityAndTabAndContentBlockSelector + " " + i + "." + EntityNavAndActionBlockCssClass )
								.ToArray() ),
						new CssElement(
							"UiEntityNavControlList",
							ControlLine.CssElementCreator.Selectors.Select( i => entityAndTabAndContentBlockSelector + " " + i + "." + EntityNavListCssClass ).ToArray() ),
						new CssElement(
							"UiEntityActionControlList",
							ControlLine.CssElementCreator.Selectors.Select( i => entityAndTabAndContentBlockSelector + " " + i + "." + EntityActionListCssClass )
								.ToArray() ),
						new CssElement( "UiEntitySummaryBlock", entityAndTabAndContentBlockSelector + " " + "div." + EntitySummaryBlockCssClass )
					};
			}

			private IEnumerable<CssElement> getSideTabAndContentElements( string entityAndTabAndContentBlockSelector ) {
				var pageActionAndContentAndContentFootCellSelector = entityAndTabAndContentBlockSelector + " td." + ContentCssClass;
				return new[]
					{
						new CssElement(
							"UiSideTabAndContentBlock",
							EwfTable.CssElementCreator.Selectors.Select( i => entityAndTabAndContentBlockSelector + " > " + i + "." + SideTabAndContentBlockCssClass )
								.ToArray() ),
						new CssElement( "UiSideTabBlockCell", entityAndTabAndContentBlockSelector + " td." + SideTabCssClass ),
						new CssElement( "UiSideTabBlock", entityAndTabAndContentBlockSelector + " div." + SideTabCssClass ),
						new CssElement( "UiSideTabGroupHead", entityAndTabAndContentBlockSelector + " div." + SideTabGroupHeadClass.ClassName ),
						new CssElement( "UiPageActionAndContentAndContentFootCell", pageActionAndContentAndContentFootCellSelector ),
						new CssElement(
							"UiPageActionControlList",
							ControlLine.CssElementCreator.Selectors.Select( i => pageActionAndContentAndContentFootCellSelector + " > " + i ).ToArray() ),
						new CssElement( "UiContentBlock", entityAndTabAndContentBlockSelector + " " + "div." + ContentCssClass ),
						new CssElement(
							"UiContentFootBlock",
							EwfTable.CssElementCreator.Selectors.Select( i => entityAndTabAndContentBlockSelector + " " + i + "." + ContentFootBlockCssClass ).ToArray() ),
						new CssElement(
							"UiContentFootActionControlList",
							ControlLine.CssElementCreator.Selectors.Select( i => entityAndTabAndContentBlockSelector + " " + i + "." + ContentFootActionListCssClass )
								.ToArray() )
					};
			}

			private IEnumerable<CssElement> getTabElements() {
				return new[]
					{
						new CssElement( "UiCurrentTabActionControl", ActionComponentCssElementCreator.Selectors.Select( i => i + "." + CurrentTabCssClass ).ToArray() ),
						new CssElement( "UiDisabledTabActionControl", ActionComponentCssElementCreator.Selectors.Select( i => i + "." + DisabledTabCssClass ).ToArray() )
					};
			}

			private IEnumerable<CssElement> getGlobalFootElements() {
				const string globalFootBlockSelector = "div#" + GlobalFootBlockId;
				return new[]
					{
						new CssElement( "UiGlobalFootBlock", globalFootBlockSelector ),
						new CssElement( "UiPoweredByEwlFooterBlock", globalFootBlockSelector + " ." + PoweredByEwlFooterClass.ClassName )
					};
			}
		}

		private IReadOnlyCollection<ActionComponentSetup> pageActions = Enumerable.Empty<ActionComponentSetup>().Materialize();
		private IReadOnlyCollection<ButtonSetup> contentFootActions = Enumerable.Empty<ButtonSetup>().Materialize();
		private Control[] contentFootControls;

		void AppEwfUiMasterPage.SetPageActions( IReadOnlyCollection<ActionComponentSetup> actions ) {
			pageActions = actions;
		}

		void AppEwfUiMasterPage.SetContentFootActions( IReadOnlyCollection<ButtonSetup> actions ) {
			contentFootActions = actions;
			contentFootControls = null;
		}

		void AppEwfUiMasterPage.SetContentFootControls( params Control[] controls ) {
			contentFootActions = null;
			contentFootControls = controls;
		}

		void ControlTreeDataLoader.LoadData() {
			EwfPage.Instance.SetContentContainer( contentPlace );

			globalPlace.AddControlsReturnThis( getGlobalBlock() );
			entityAndTopTabPlace.AddControlsReturnThis( getEntityAndTopTabBlock() );
			if( entityUsesTabMode( TabMode.Vertical ) )
				setUpSideTabs();
			pageActionPlace.AddControlsReturnThis( getPageActionList() );
			var contentFootBlock = getContentFootBlock();
			if( contentFootBlock != null )
				contentFootPlace.AddControlsReturnThis( contentFootBlock );
			var globalFootBlock = getGlobalFootBlock();
			if( globalFootBlock != null )
				globalFootPlace.AddControlsReturnThis( globalFootBlock );

			if( !EwfUiStatics.AppProvider.BrowserWarningDisabled() ) {
				if( AppRequestState.Instance.Browser.IsOldVersionOfMajorBrowser() && !StandardLibrarySessionState.Instance.HideBrowserWarningForRemainderOfSession )
					EwfPage.AddStatusMessage(
						StatusMessageType.Warning,
						StringTools.ConcatenateWithDelimiter(
							" ",
							"We've detected that you are not using the latest version of your browser.",
							"While most features of this site will work, and you will be safe browsing here, we strongly recommend using the newest version of your browser in order to provide a better experience on this site and a safer experience throughout the Internet." ) +
						"<br/>" +
						NetTools.BuildBasicLink( "Click here to get Firefox (it's free)", new ExternalResourceInfo( "http://www.getfirefox.com" ).GetUrl(), true ) +
						"<br />" +
						NetTools.BuildBasicLink(
							"Click here to get Chrome (it's free)",
							new ExternalResourceInfo( "https://www.google.com/intl/en/chrome/browser/" ).GetUrl(),
							true ) + "<br />" + NetTools.BuildBasicLink(
							"Click here to get the latest Internet Explorer (it's free)",
							new ExternalResourceInfo( "http://www.beautyoftheweb.com/" ).GetUrl(),
							true ) );
				StandardLibrarySessionState.Instance.HideBrowserWarningForRemainderOfSession = true;
			}
		}

		private Control getGlobalBlock() {
			// ReSharper disable once SuspiciousTypeConversion.Global
			var appLogoAndUserInfoControlOverrider = EwfUiStatics.AppProvider as AppLogoAndUserInfoControlOverrider;

			return new Block(
				new[]
						{
							appLogoAndUserInfoControlOverrider != null ? appLogoAndUserInfoControlOverrider.GetAppLogoAndUserInfoControl() : getAppLogoAndUserInfoBlock(),
							getGlobalNavBlock(),
							new PlaceHolder().AddControlsReturnThis(
								new FlowErrorContainer(
										new ErrorSourceSet( includeGeneralErrors: true ),
										new ListErrorDisplayStyle( classes: CssElementCreator.TopErrorMessageListContainerClass ) ).ToCollection()
									.GetControls() )
						}.Where( i => i != null )
					.ToArray() ) { ClientIDMode = ClientIDMode.Static, ID = CssElementCreator.GlobalBlockId };
		}

		private Control getAppLogoAndUserInfoBlock() {
			var table = EwfTable.Create( style: EwfTableStyle.StandardLayoutOnly, classes: CssElementCreator.AppLogoAndUserInfoBlockCssClass.ToCollection() );

			var appLogo = new GenericFlowContainer(
				( !ConfigurationStatics.IsIntermediateInstallation || AppRequestState.Instance.IntermediateUserExists
					  ? EwfUiStatics.AppProvider.GetLogoComponent()
					  : null ) ?? ( EwfApp.Instance.AppDisplayName.Length > 0 ? EwfApp.Instance.AppDisplayName : ConfigurationStatics.SystemName ).ToComponents(),
				classes: CssElementCreator.AppLogoClass );

			ControlStack userInfoList = null;
			if( AppRequestState.Instance.UserAccessible ) {
				var changePasswordPage = UserManagement.ChangePassword.Page.GetInfo( EwfPage.Instance.InfoAsBaseType.GetUrl() );
				if( changePasswordPage.UserCanAccessResource && AppTools.User != null ) {
					var userInfo = new UserInfo();
					userInfo.LoadData( changePasswordPage );
					userInfoList = ControlStack.CreateWithControls( true, userInfo );
					userInfoList.CssClass = CssElementCreator.UserInfoListCssClass;
				}
			}

			table.AddItem( () => new EwfTableItem( new PlaceHolder().AddControlsReturnThis( appLogo.ToCollection().GetControls() ), userInfoList ) );
			return table;
		}

		private Control getGlobalNavBlock() {
			// This check exists to prevent the display of lookup boxes or other post back controls. With these controls we sometimes don't have a specific
			// destination page to use for an authorization check, meaning that the system code has no way to prevent their display when there is no intermediate
			// user.
			if( ConfigurationStatics.IsIntermediateInstallation && !AppRequestState.Instance.IntermediateUserExists )
				return null;

			var formItems = EwfUiStatics.AppProvider.GetGlobalNavFormControls()
				.Select( ( control, index ) => control.GetFormItem( PostBack.GetCompositeId( "global", "nav", index.ToString() ) ) )
				.Materialize();
			var controls = getActionControls( EwfUiStatics.AppProvider.GetGlobalNavActions() ).Concat( formItems.Select( i => i.Control ) ).ToArray();
			if( !controls.Any() )
				return null;

			return new Block(
				new ControlLine( controls ) { ItemsSeparatedWithPipe = EwfUiStatics.AppProvider.GlobalNavItemsSeparatedWithPipe() }.ToCollection()
					.Concat(
						new FlowErrorContainer( new ErrorSourceSet( validations: formItems.Select( i => i.Validation ) ), new ListErrorDisplayStyle() ).ToCollection()
							.GetControls() )
					.ToArray() ) { CssClass = CssElementCreator.GlobalNavBlockCssClass };
		}

		private Control getEntityAndTopTabBlock() {
			var controls = new List<Control> { getEntityBlock() };
			if( entityUsesTabMode( TabMode.Horizontal ) ) {
				var resourceGroups = EwfPage.Instance.InfoAsBaseType.EsInfoAsBaseType.Resources;
				if( resourceGroups.Count > 1 )
					throw new ApplicationException( "Top tabs are not supported with multiple resource groups." );
				controls.Add( getTopTabBlock( resourceGroups.Single() ) );
			}
			return new Block( controls.ToArray() ) { CssClass = CssElementCreator.EntityAndTopTabBlockCssClass };
		}

		private Control getEntityBlock() {
			return new Block( new[] { getPagePath(), getEntityNavAndActionBlock(), getEntitySummaryBlock() }.Where( i => i != null ).ToArray() )
				{
					CssClass = CssElementCreator.EntityBlockCssClass
				};
		}

		private Control getPagePath() {
			var entitySetupInfo = EwfPage.Instance.InfoAsBaseType.EsInfoAsBaseType;
			var pagePath = new PagePath(
				currentPageBehavior: entitySetupInfo != null && EwfPage.Instance.InfoAsBaseType.ParentResource == null && entitySetupInfo.Resources.Any()
					                     ? PagePathCurrentPageBehavior.IncludeCurrentPageAndExcludePageNameIfEntitySetupExists
					                     : PagePathCurrentPageBehavior.IncludeCurrentPage );
			return pagePath.IsEmpty ? null : pagePath;
		}

		private Control getEntityNavAndActionBlock() {
			var cells = new[] { getEntityNavCell(), getEntityActionCell() }.Where( i => i != null );
			if( !cells.Any() )
				return null;
			var table = EwfTable.Create( style: EwfTableStyle.Raw, classes: CssElementCreator.EntityNavAndActionBlockCssClass.ToCollection() );
			table.AddItem( new EwfTableItem( cells ) );
			return table;
		}

		private EwfTableCell getEntityNavCell() {
			if( uiEntitySetup == null )
				return null;

			var formItems = uiEntitySetup.GetNavFormControls()
				.Select( ( control, index ) => control.GetFormItem( PostBack.GetCompositeId( "entity", "nav", index.ToString() ) ) )
				.Materialize();
			var controls = getActionControls( uiEntitySetup.GetNavActions() ).Concat( formItems.Select( i => i.Control ) ).ToArray();
			if( !controls.Any() )
				return null;

			return new ControlLine( controls )
					{
						CssClass = CssElementCreator.EntityNavListCssClass, ItemsSeparatedWithPipe = EwfUiStatics.AppProvider.EntityNavAndActionItemsSeparatedWithPipe()
					}.ToCollection()
				.Concat(
					new FlowErrorContainer( new ErrorSourceSet( validations: formItems.Select( i => i.Validation ) ), new ListErrorDisplayStyle() ).ToCollection()
						.GetControls() )
				.ToCell();
		}

		private EwfTableCell getEntityActionCell() {
			if( uiEntitySetup == null || EwfPage.Instance.InfoAsBaseType.ParentResource != null )
				return null;
			var actionControls = getActionControls( uiEntitySetup.GetActions() ).ToArray();
			if( !actionControls.Any() )
				return null;
			return new ControlLine( actionControls )
				{
					CssClass = CssElementCreator.EntityActionListCssClass, ItemsSeparatedWithPipe = EwfUiStatics.AppProvider.EntityNavAndActionItemsSeparatedWithPipe()
				}.ToCell( new TableCellSetup( textAlignment: TextAlignment.Right ) );
		}

		private Control getEntitySummaryBlock() {
			// If the entity setup is a nonempty control, display it as an entity summary.
			//
			// It's a hack to call GetDescendants this early in the life cycle, but we should be able to fix it when we separate EWF from Web Forms. This is
			// EnduraCode goal 790. What we are essentially doing here is determining whether there is at least one "component" in the entity summary.
			if( EwfPage.Instance.EsAsBaseType is Control entitySummary && EwfPage.Instance.GetDescendants( entitySummary ).Any( i => !( i is PlaceHolder ) ) )
				return new Block( entitySummary ) { CssClass = CssElementCreator.EntitySummaryBlockCssClass };

			return null;
		}

		private Control getTopTabBlock( ResourceGroup resourceGroup ) {
			return new Block(
				new ControlLine( getTabControlsForResources( resourceGroup, false ).ToArray() )
					{
						CssClass = CssElementCreator.TopTabCssClass, VerticalAlignment = TableCellVerticalAlignment.Bottom
					} ) { CssClass = CssElementCreator.TopTabCssClass };
		}

		private bool entityUsesTabMode( TabMode tabMode ) {
			var entitySetupInfo = EwfPage.Instance.InfoAsBaseType.EsInfoAsBaseType;
			return entitySetupInfo != null && EwfPage.Instance.InfoAsBaseType.ParentResource == null && entitySetupInfo.GetTabMode() == tabMode;
		}

		private void setUpSideTabs() {
			sideTabCell.Visible = true;
			var controls = new List<Control>();
			foreach( var resourceGroup in EwfPage.Instance.InfoAsBaseType.EsInfoAsBaseType.Resources ) {
				var tabs = getTabControlsForResources( resourceGroup, true );
				if( tabs.Any() && resourceGroup.Name.Any() )
					controls.AddRange(
						new GenericFlowContainer( resourceGroup.Name.ToComponents(), classes: CssElementCreator.SideTabGroupHeadClass ).ToCollection().GetControls() );
				controls.AddRange( tabs );
			}
			sideTabCell.AddControlsReturnThis( new Block( controls.ToArray() ) { CssClass = CssElementCreator.SideTabCssClass } );
		}

		private IEnumerable<Control> getTabControlsForResources( ResourceGroup resourceGroup, bool includeIcons ) {
			var tabs = new List<Control>();
			foreach( var resource in resourceGroup.Resources.Where( p => p.UserCanAccessResource ) ) {
				var tab = EwfLink.Create(
					resource.IsIdenticalToCurrent() ? null : resource,
					new TextActionControlStyle(
						resource.ResourceName,
						icon: includeIcons ? new ActionComponentIcon( new FontAwesomeIcon( resource.IsIdenticalToCurrent() ? "fa-circle" : "fa-circle-thin" ) ) : null ) );

				tab.CssClass = resource.IsIdenticalToCurrent() ? CssElementCreator.CurrentTabCssClass :
				               resource.AlternativeMode is DisabledResourceMode ? CssElementCreator.DisabledTabCssClass : "";
				tabs.Add( tab );
			}
			return tabs;
		}

		private IEnumerable<Control> getPageActionList() {
			var actionControls = getActionControls( pageActions ).ToArray();
			if( !actionControls.Any() )
				yield break;
			yield return new ControlLine( actionControls ) { ItemsSeparatedWithPipe = EwfUiStatics.AppProvider.PageActionItemsSeparatedWithPipe() };
		}

		private IEnumerable<Control> getActionControls( IReadOnlyCollection<ActionComponentSetup> actions ) =>
			from action in actions
			let actionComponent =
				action.GetActionComponent(
					( text, icon ) => new StandardHyperlinkStyle( text, icon: icon ),
					( text, icon ) => new StandardButtonStyle( text, buttonSize: ButtonSize.ShrinkWrap, icon: icon ) )
			where actionComponent != null
			select new PlaceHolder().AddControlsReturnThis( actionComponent.ToCollection().GetControls() );

		private Control getContentFootBlock() {
			var controls = new List<Control>();
			if( contentFootActions != null ) {
				if( contentFootActions.Any() )
					controls.Add(
						new ControlLine(
							contentFootActions.Select(
									( action, index ) => (Control)new PlaceHolder().AddControlsReturnThis(
										action.GetActionComponent(
												null,
												( text, icon ) => new StandardButtonStyle( text, buttonSize: ButtonSize.Large, icon: icon ),
												enableSubmitButton: index == 0 )
											.ToCollection()
											.GetControls() ) )
								.ToArray() ) { CssClass = CssElementCreator.ContentFootActionListCssClass } );
				else if( EwfPage.Instance.IsAutoDataUpdater )
					controls.Add( new PostBackButton( new ButtonActionControlStyle( "Update Now" ), postBack: EwfPage.Instance.DataUpdatePostBack ) );
			}
			else {
				if( EwfPage.Instance.IsAutoDataUpdater )
					throw new ApplicationException( "AutoDataUpdater is not currently compatible with custom content foot controls." );
				controls.AddRange( contentFootControls.ToList() );
			}

			if( !controls.Any() )
				return null;

			var table = EwfTable.Create( style: EwfTableStyle.StandardLayoutOnly, classes: CssElementCreator.ContentFootBlockCssClass.ToCollection() );
			table.AddItem(
				new EwfTableItem(
					controls.ToCell(
						new TableCellSetup( textAlignment: contentFootActions != null && contentFootActions.Any() ? TextAlignment.Right : TextAlignment.Center ) ) ) );
			return table;
		}

		private Control getGlobalFootBlock() {
			var controls = new List<Control>();

			// This check exists to prevent the display of post back controls. With these controls we sometimes don't have a specific destination page to use for an
			// authorization check, meaning that the system code has no way to prevent their display when there is no intermediate user.
			if( !ConfigurationStatics.IsIntermediateInstallation || AppRequestState.Instance.IntermediateUserExists )
				controls.AddRange( EwfUiStatics.AppProvider.GetGlobalFootControls() );

			var ewlWebSite = new ExternalResourceInfo( "http://enterpriseweblibrary.org/" );
			if( ewlWebSite.UserCanAccessResource && !EwfUiStatics.AppProvider.PoweredByEwlFooterDisabled() )
				controls.AddRange(
					new Paragraph(
							"Powered by the ".ToComponents()
								.Append( new EwfHyperlink( ewlWebSite.ToHyperlinkNewTabBehavior(), new StandardHyperlinkStyle( EwlStatics.EwlName ) ) )
								.Concat(
									" ({0} version)".FormatWith( TimeZoneInfo.ConvertTime( EwlStatics.EwlBuildDateTime, TimeZoneInfo.Local ).ToMonthYearString() )
										.ToComponents() )
								.Materialize(),
							classes: CssElementCreator.PoweredByEwlFooterClass ).ToCollection()
						.GetControls() );

			return controls.Any() ? new Block( controls.ToArray() ) { ClientIDMode = ClientIDMode.Static, ID = CssElementCreator.GlobalFootBlockId } : null;
		}

		private UiEntitySetupBase uiEntitySetup => EwfPage.Instance.EsAsBaseType as UiEntitySetupBase;
	}
}