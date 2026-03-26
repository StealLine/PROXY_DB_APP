namespace PROXY_TODB.Models
{
	public class FullProgramModel
	{
		public static string Host { get; set; } = string.Empty;
		public static string Port { get; set; } = string.Empty;
		public static string MasterConnectionString { get; set; } = string.Empty;
		public static string ManagementDBName { get; set; } = string.Empty;
		public static string ManagementConnectionString { get; set; } = string.Empty;
	}
}