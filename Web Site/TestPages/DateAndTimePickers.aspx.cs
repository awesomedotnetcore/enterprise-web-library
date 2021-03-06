using EnterpriseWebLibrary.EnterpriseWebFramework;
using EnterpriseWebLibrary.EnterpriseWebFramework.Controls;

namespace EnterpriseWebLibrary.WebSite.TestPages {
	partial class DateAndTimePickers: EwfPage {
		partial class Info {
			public override string ResourceName => "Date/Time Controls";
		}

		protected override void loadData() {
			var table = FormItemBlock.CreateFormItemTable();
			table.AddFormItems(
				new DateControl( null, true ).ToFormItem( label: "Date control".ToComponents() ),
				FormItem.Create( "Time Picker", new TimePicker( null ) ),
				new DateAndTimeControl( null, true ).ToFormItem( label: "Date and time control".ToComponents() ),
				new DurationControl( null, true ).ToFormItem( label: "Duration control".ToComponents() ) );
			ph.AddControlsReturnThis( table );
		}

		public override bool IsAutoDataUpdater => true;
	}
}