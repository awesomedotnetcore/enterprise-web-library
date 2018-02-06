﻿using System.Collections.Generic;
using System.Linq;
using Humanizer;
using JetBrains.Annotations;

namespace EnterpriseWebLibrary.EnterpriseWebFramework {
	/// <summary>
	/// A modal box.
	/// </summary>
	public class ModalBox: EtherealComponent {
		private static readonly ElementClass boxClass = new ElementClass( "ewfMdl" );
		private static readonly ElementClass closeButtonContainerClass = new ElementClass( "ewfMdlB" );
		private static readonly ElementClass contentContainerClass = new ElementClass( "ewfMdlC" );

		[ UsedImplicitly ]
		private class CssElementCreator: ControlCssElementCreator {
			IReadOnlyCollection<CssElement> ControlCssElementCreator.CreateCssElements() {
				return new[]
					{
						new CssElement( "ModalBox", "div.{0}".FormatWith( boxClass.ClassName ) ),
						new CssElement( "ModalBoxCloseButtonContainer", "div.{0}".FormatWith( closeButtonContainerClass.ClassName ) ),
						new CssElement( "ModalBoxContentContainer", "div.{0}".FormatWith( contentContainerClass.ClassName ) ),
						new CssElement( "ModalBoxContainer", "dialog.{0}".FormatWith( boxClass.ClassName ) ),
						new CssElement( "ModalBoxBackdrop", "dialog.{0}::backdrop".FormatWith( boxClass.ClassName ), "dialog.{0} + .backdrop".FormatWith( boxClass.ClassName ) )
					};
			}
		}

		internal static IEnumerable<EtherealComponent> CreateBrowsingModalBox() {
			return new ModalBox( EwfPage.Instance.BrowsingModalBoxId, true, Enumerable.Empty<FlowComponent>() ).ToCollection();
		}

		internal static string GetBrowsingModalBoxOpenStatements( BrowsingContextSetup browsingContextSetup, string url ) {
			browsingContextSetup = browsingContextSetup ?? new BrowsingContextSetup();
			return StringTools.ConcatenateWithDelimiter(
				" ",
				"var dl = document.getElementById( '{0}' );".FormatWith( EwfPage.Instance.BrowsingModalBoxId.ElementId.Id ),
				"var dv = dl.firstElementChild;",
				"$( dv ).children( 'iframe' ).remove();",
				"var fr = document.createElement( 'iframe' );",
				"fr.src = '{0}';".FormatWith( url ),
				"fr.style.width = '{0}';".FormatWith( browsingContextSetup.Width?.Value ?? "" ),
				"fr.style.height = '{0}';".FormatWith( browsingContextSetup.Height?.Value ?? "" ),
				"dv.insertAdjacentElement( 'beforeend', fr );",
				"dl.showModal();" );
		}

		private readonly IReadOnlyCollection<EtherealComponent> children;

		/// <summary>
		/// Creates a modal box.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="includeCloseButton"></param>
		/// <param name="content"></param>
		/// <param name="open"></param>
		public ModalBox( ModalBoxId id, bool includeCloseButton, IEnumerable<FlowComponent> content, bool open = false ) {
			children = new ElementComponent(
				context => {
					id.ElementId.AddId( context.Id );
					return new ElementData(
						() => new ElementLocalData(
							"dialog",
							includeIdAttribute: true,
							jsInitStatements: ( includeCloseButton
								                    ? "$( '#{0}' ).click( function( e ) {{ if( e.target.id === '{0}' ) e.target.close(); }} );".FormatWith( context.Id )
								                    : "" ).ConcatenateWithSpace( open ? "document.getElementById( '{0}' ).showModal();".FormatWith( context.Id ) : "" ) ),
						classes: boxClass,
						children: new GenericFlowContainer(
							( includeCloseButton
								  ? new GenericFlowContainer(
									  new EwfButton(
										  new StandardButtonStyle( "Close", icon: new ActionComponentIcon( new FontAwesomeIcon( "fa-times" ) ) ),
										  behavior: new CustomButtonBehavior( () => "document.getElementById( '{0}' ).close();".FormatWith( context.Id ) ) ).ToCollection(),
									  classes: closeButtonContainerClass ).ToCollection<FlowComponent>()
								  : Enumerable.Empty<FlowComponent>() ).Concat(
								id == EwfPage.Instance.BrowsingModalBoxId ? content : new GenericFlowContainer( content, classes: contentContainerClass ).ToCollection() ),
							classes: boxClass ).ToCollection() );
				} ).ToCollection();
		}

		IEnumerable<EtherealComponentOrElement> EtherealComponent.GetChildren() {
			return children;
		}
	}
}