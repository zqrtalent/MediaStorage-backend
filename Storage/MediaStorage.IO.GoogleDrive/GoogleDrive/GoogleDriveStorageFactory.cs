namespace MediaStorage.IO.GoogleDrive
{
    public  static class GoogleDriveStorageFactory
    {
        public static GoogleDriveStorage Create(string appName, string serviceAccessCredentialsJsonFile)
        {
            return new GoogleDriveStorage(appName, serviceAccessCredentialsJsonFile);
        }

        public static GoogleDriveStorage Create(string appName, string clientId, string clientSecret)
        {
            return new GoogleDriveStorage(appName, clientId, clientSecret);
        }
    }

}