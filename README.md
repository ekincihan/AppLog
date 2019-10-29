# AppLog
Graylog GELFT UDP Logging

# appsetting.json 
  "AppLog-ApplicationId": "{AppLog.Sample.ApplicationId}",\
  "AppLog-LoggerName": "{AppLog.Sample.LoggerName}",\
  "AppLog-GrayLogHost": "{GrayLog API Endpoint}",\
  "AppLog-GrayLogPort": 12201, \
  "AppLog-MinimumLogLevel": "Debug",
  
# Code
  await _logger.Current().InfoAsync("test");\
  _logger.Current().Error(ex, "test", 1);
