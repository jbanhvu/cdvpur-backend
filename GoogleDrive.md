# Google Drive Manual Test

## 1. Run API

Start the backend API from the `ChangdaeVinaPurchasingApi` project:

```bash
dotnet run --project ChangdaeVinaPurchasingApi/ChangdaeVinaPurchasingApi.csproj
```

Make sure `appsettings.json` contains the real Google Drive folder id:

```json
"GoogleDrive": {
  "ClientId": "YOUR_CLIENT_ID",
  "ClientSecret": "YOUR_CLIENT_SECRET",
  "RefreshToken": "YOUR_REFRESH_TOKEN",
  "FolderId": "YOUR_GOOGLE_DRIVE_FOLDER_ID"
}
```

The files will be uploaded as the Google account that granted the refresh token.

## 2. Check Health

Call:

```http
GET /api/google-drive/health
```

Expected success response:

```json
{
  "success": true,
  "folderId": "...",
  "serviceAccount": "...",
  "message": "Google Drive configuration is valid."
}
```

If this fails, check:

- `GoogleDrive:ClientId` is configured.
- `GoogleDrive:ClientSecret` is configured.
- `GoogleDrive:RefreshToken` is valid and not expired or revoked.
- `GoogleDrive:FolderId` is not `REPLACE_FOLDER_ID`.
- The OAuth user can access the configured folder.
- Google Drive API is enabled for the Google Cloud project.

## 3. Upload File

Call:

```http
POST /api/google-drive/upload
Content-Type: multipart/form-data
```

Form-data:

```text
file: select a file less than or equal to 20 MB
```

Expected response:

```json
{
  "fileId": "...",
  "fileName": "...",
  "fileUrl": "https://drive.google.com/uc?id=...",
  "fileSize": 12345
}
```

## 4. Verify Google Drive

Open the configured Google Drive folder and confirm the uploaded file appears there.

## 5. Open FileUrl

Open `fileUrl` in a browser. The file should be publicly readable because the API grants `anyone/reader`.

## 6. Download File

Call:

```http
GET /api/google-drive/download/{fileId}
```

The API should stream the file content back to the client.

## 7. Delete File

Call:

```http
DELETE /api/google-drive/{fileId}
```

Expected response:

```json
{
  "success": true
}
```

## 8. Verify Delete

Open Google Drive and confirm the file no longer appears in the configured folder.
