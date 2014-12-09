using System;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace FixOne.Server.Services.ConfigurationService
{
	[ServiceContract]
	public interface IConfigurationService
	{
		[OperationContract, WebGet]
		string GetData();

		[OperationContract, WebGet]
		string[] GetArray();

		[OperationContract, WebGet]
		string GetEngineVersion ();

		[OperationContract, WebGet]
		Status RestartEngine ();

		[OperationContract, WebGet]
		bool IsEngineRunning ();
	}
}

