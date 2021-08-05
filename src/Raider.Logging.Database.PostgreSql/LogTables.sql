CREATE TABLE aud."LogMessage" (
	"IdLogMessage"  bigint NOT NULL   DEFAULT NEXTVAL(('aud."logmessage_idlogmessage_seq"'::text)::regclass),
	"IdLogLevel" int4 NOT NULL,
	"Created" timestamp NOT NULL,
	"RuntimeUniqueKey" uuid NOT NULL,
	"LogCode" varchar(31) NULL,
	"ClientMessage" text NULL,
	"InternalMessage" text NULL,
	"MethodCallId" uuid  NOT NULL,
	"SourceContext" text NULL,
	"TraceFrame" text NULL,
	"StackTrace" text NULL,
	"Detail" text  NULL,
	"IdUser" int4  NULL,
	"CommandQueryName" varchar(1023)  NULL,
	"IdCommandQuery" bigint NULL,
	"MethodCallElapsedMilliseconds" numeric NULL,
	"PropertyName" varchar(255)  NULL,
	"DisplayPropertyName" varchar(255)  NULL,
	"ValidationFailure" text NULL,
	"IsValidationError" bool  NULL,
	"CustomData" text NULL,
	"Tags" text NULL,
	"CorrelationId" uuid  NULL
);
GRANT INSERT, SELECT, UPDATE, DELETE ON TABLE aud."LogMessage" TO postgres;
CREATE SEQUENCE aud."logmessage_idlogmessage_seq" INCREMENT 1 START 1;


CREATE TABLE aud."Log" (
	"IdLog"  bigint NOT NULL   DEFAULT NEXTVAL(('aud."log_idlog_seq"'::text)::regclass),
	"IdLogLevel" int4 NOT NULL,
	"Created" timestamp NOT NULL,
	"InternalMessage" text NULL,
	"Detail" text  NULL,
	"StackTrace" text NULL,
	"MethodCallId" uuid NULL,
	"SourceContext" text NULL
);
GRANT INSERT, SELECT, UPDATE, DELETE ON TABLE aud."Log" TO postgres;
CREATE SEQUENCE aud."log_idlog_seq" INCREMENT 1 START 1;


CREATE TABLE aud."HardwareInfo" (
	"IdHardwareInfo" int4 NOT NULL,
	"CreatedAt" timestamp NOT NULL,
	"RuntimeUniqueKey" uuid NOT NULL,
	"HWThumbprint" varchar(63) NOT NULL,
	"TotalMemoryCapacityGB" numeric NULL,
	"MemoryAvailableGB" numeric NULL,
	"MemoryPercentUsed" numeric NULL,
	"PercentProcessorIdleTime" numeric NULL,
	"PercentProcessorTime" numeric NULL,
	"OS" varchar(4095)  NULL,
	"HWJson" text NULL
);
GRANT INSERT, SELECT, UPDATE, DELETE ON TABLE aud."EnvironmentInfo" TO xyzusr;



CREATE TABLE aud."EnvironmentInfo" (
	"IdEnvironmentInfo" int4 NOT NULL,
	"Created" timestamp NOT NULL,
	"RuntimeUniqueKey" uuid NOT NULL,
	"RunningEnvironment" varchar(255) NULL,
	"FrameworkDescription" varchar(255) NULL,
	"TargetFramework" varchar(255) NULL,
	"CLRVersion" varchar(255) NULL,
	"EntryAssemblyName" varchar(255) NULL,
	"EntryAssemblyVersion" varchar(255) NULL,
	"BaseDirectory" varchar(255) NULL,
	"MachineName" varchar(255) NULL,
	"CurrentAppDomainName" varchar(255) NULL,
	"Is64BitOperatingSystem" bool NULL,
	"Is64BitProcess" bool NULL,
	"OperatingSystemArchitecture" varchar(255) NULL,
	"OperatingSystemPlatform" varchar(255) NULL,
	"OperatingSystemVersion" varchar(255) NULL,
	"ProcessArchitecture" varchar(255)  NULL,
	"CommandLine" varchar(255) NULL
);
GRANT INSERT, SELECT, UPDATE, DELETE ON TABLE aud."EnvironmentInfo" TO xyzusr;