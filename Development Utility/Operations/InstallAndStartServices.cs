﻿using System.Collections.Generic;
using EnterpriseWebLibrary.InstallationSupportUtility;
using EnterpriseWebLibrary.InstallationSupportUtility.InstallationModel;

namespace EnterpriseWebLibrary.DevelopmentUtility.Operations {
	internal class InstallAndStartServices: Operation {
		private static readonly Operation instance = new InstallAndStartServices();
		public static Operation Instance { get { return instance; } }
		private InstallAndStartServices() {}

		bool Operation.IsValid( Installation installation ) {
			return installation is RecognizedDevelopmentInstallation;
		}

		void Operation.Execute( Installation genericInstallation, IReadOnlyList<string> arguments, OperationResult operationResult ) {
			var installation = genericInstallation as RecognizedDevelopmentInstallation;
			installation.ExistingInstallationLogic.InstallServices();
			installation.ExistingInstallationLogic.Start();
		}
	}
}