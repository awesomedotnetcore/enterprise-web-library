﻿using System;
using System.Collections.Generic;
using EnterpriseWebLibrary.InputValidation;
using Humanizer;

namespace EnterpriseWebLibrary.EnterpriseWebFramework {
	/// <summary>
	/// A hidden field.
	/// </summary>
	public class EwfHiddenField: FormControl<EtherealComponent> {
		private readonly EtherealComponent component;
		private readonly EwfValidation validation;

		/// <summary>
		/// Creates a hidden field.
		/// </summary>
		/// <param name="value">Do not pass null.</param>
		/// <param name="id"></param>
		/// <param name="pageModificationValue"></param>
		/// <param name="validationMethod">The validation method. Pass null if you’re only using this control for page modification.</param>
		/// <param name="jsInitStatementGetter">A function that takes the field’s ID and returns the JavaScript statements that should be executed when the DOM is
		/// loaded. Do not return null.</param>
		public EwfHiddenField(
			string value, HiddenFieldId id = null, PageModificationValue<string> pageModificationValue = null,
			Action<PostBackValue<string>, Validator> validationMethod = null, Func<string, string> jsInitStatementGetter = null ) {
			pageModificationValue = pageModificationValue ?? new PageModificationValue<string>();

			var elementId = new ElementId();
			var formValue = new FormValue<string>(
				() => value,
				() => elementId.Id,
				v => v,
				rawValue => rawValue != null ? PostBackValueValidationResult<string>.CreateValid( rawValue ) : PostBackValueValidationResult<string>.CreateInvalid() );

			component = new ElementComponent(
				context => {
					elementId.AddId( context.Id );
					id?.AddId( context.Id );
					return new ElementData(
						() => {
							var attributes = new List<Tuple<string, string>>();
							attributes.Add( Tuple.Create( "type", "hidden" ) );
							attributes.Add( Tuple.Create( "name", context.Id ) );
							attributes.Add( Tuple.Create( "value", pageModificationValue.Value ) );

							return new ElementLocalData(
								"input",
								focusDependentData: new ElementFocusDependentData(
									attributes: attributes,
									includeIdAttribute: id != null || pageModificationValue != null || jsInitStatementGetter != null,
									jsInitStatements: StringTools.ConcatenateWithDelimiter(
										" ",
										pageModificationValue != null
											? "$( '#{0}' ).change( function() {{ {1} }} );".FormatWith( context.Id, pageModificationValue.GetJsModificationStatements( "$( this ).val()" ) )
											: "",
										jsInitStatementGetter?.Invoke( context.Id ) ?? "" ) ) );
						} );
				},
				formValue: formValue );

			formValue.AddPageModificationValue( pageModificationValue, v => v );

			if( validationMethod != null )
				validation = formValue.CreateValidation( validationMethod );
		}

		FormControlLabeler FormControl<EtherealComponent>.Labeler => null;
		public EtherealComponent PageComponent => component;
		public EwfValidation Validation => validation;
	}
}