using System;
using CommandLine;
using CommandLine.Text;
using CxAnalytix.Configuration.Impls;
using CxAnalytix.MongoTool;
using CxAnalytix.Out.MongoDBOutput.Config.Impl;
using log4net;
using log4net.Core;
using MongoDB.Driver;


var parser = new Parser(settings => { settings.HelpWriter = null; });
var parsedData = parser.ParseArguments<CommandLineOpts>(args);


var logPattern = new log4net.Layout.PatternLayout("[%-5level][%utcdate{yyyy-MM-ddTHH:mm:ss.fffZ}][%c] %message%newline");
var repo = LogManager.GetRepository() as log4net.Repository.Hierarchy.Hierarchy;
var file = new log4net.Appender.RollingFileAppender() { Layout = logPattern, File = "MongoTool.log", AppendToFile = true };
file.ActivateOptions();
repo.Root.AddAppender(file);
repo.Root.AddAppender(new log4net.Appender.ConsoleAppender() { Layout = logPattern });
repo.Configured = true;

Console.WriteLine("CxAnalytix MongoDB Schema Tool");
Console.WriteLine("Run with --help to see command line options.");


parsedData.WithParsed((opts) =>
{
    if (opts.Debug)
        LogManager.GetRepository().Threshold = Level.Debug;
    else
        LogManager.GetRepository().Threshold = Level.Info;

    var _log = LogManager.GetLogger(typeof(Program));

    _log.Info("START");

    if (opts.UseConfigConnectionString && opts.MongoConnectionString == null)
        try
        {
            _log.Info("Loading MongoDB connection string from the CxAnalytix configuration file.");

            var mongo_config = Config.GetConfig<MongoConnectionConfig>();

            if (mongo_config != null)
                opts.MongoConnectionString = new MongoUrl(mongo_config.ConnectionString);
        }
        catch (Exception ex)
        {
            _log.Error("Failed to load the MongoDB connection URL from the CxAnalytix configuration file.");

            _log.Debug(ex.Message);
            _log.Debug(ex.StackTrace);
            if (ex.InnerException != null)
                _log.Debug(ex.InnerException.Message);
        }
    else
    {
        _log.Error("Do not supply a connection string if indicating it should load from the CxAnalytix configuration file.");
        Environment.Exit(1);
    }

    if (opts.MongoConnectionString == null)
    {
        _log.Error("A MongoDB connection URL is required.");
        Environment.Exit(1);
    }
    else
    {
        try
        {
            _log.Info($"Operations are using [{opts.LoginDB}] as the login database.");

            UserCreator.CreateOrUpdateUser(opts.MongoConnectionString, opts.LoginDB);

            if (!String.IsNullOrEmpty(opts.MongoUser) && !String.IsNullOrEmpty(opts.MongoPassword))
                UserCreator.CreateOrUpdateUser(opts.MongoConnectionString, opts.LoginDB, opts.MongoUser, opts.MongoPassword);

            CollectionCreator.CreateCollections(opts.MongoConnectionString);
        }
        catch (Exception ex)
        {
            _log.Error(ex.Message);
            _log.Debug(ex.StackTrace);
        }
    }

    _log.Info("END");


}).WithNotParsed(errors =>
{
    var helpText = HelpText.AutoBuild<CommandLineOpts>(parsedData, h => HelpText.DefaultParsingErrorsHandler(parsedData, h), e=> e);
    Console.WriteLine(helpText);
    Environment.Exit(1);
});



public class CommandLineOpts
{
    [Option('u', "url", SetName="connection", Default = null, Required = false, HelpText = "Use this MongoDB connection URL.")]
    public MongoUrl MongoConnectionString { get; set; }

    [Option('c', SetName = "connection", Default = false, Required = false, HelpText = "Load the MongoDB connection URL from the CxAnalytix configuration file.")]
    public bool UseConfigConnectionString { get; set; }

    [Option('d', "debug", Default = false, Required = false, HelpText = "Enable debug output.")]
    public bool Debug { get; set; }

    [Option('l', "login-db", Default = "admin", Required = false, HelpText = "Specify the MongoDB database used for login.")]
    public String LoginDB { get; set; }

    [Option("mongo-user", Default = null, Required = false, HelpText = "The name of a MongoDB user to create and assign to the CxAnalytix database with minimal privileges.")]
    public String MongoUser { get; set; }

    [Option("mongo-password", Default = null, Required = false, HelpText = "The password for the MongoDB user that is created.")]
    public String MongoPassword { get; set; }

}



