using PrincipleStudios.OpenApiCodegen.Server.Mvc.TestApp;

Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseStartup<Startup>();
    });