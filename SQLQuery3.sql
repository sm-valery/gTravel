USE [C:\PROJECTS\GTRAVEL\WEBAPPLICATION1\APP_DATA\GODB.MDF]
GO

DECLARE	@return_value Int

EXEC	@return_value = [dbo].[cleardb]

SELECT	@return_value as 'Return Value'

GO
