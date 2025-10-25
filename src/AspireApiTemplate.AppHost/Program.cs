using System.Net.Sockets;

var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume();

var db = sql.AddDatabase("DBTEMPLATENAME");

var seeder = builder.AddProject<Projects.AspireApiTemplate_Seeder>("DatabaseSeeder")
    .WithIconName("Database")
    .WithReference(db)
    .WaitFor(db);

var server = builder.AddProject<Projects.AspireApiTemplate_WebAPI>("WebAPI")
    .WithIconName("Server")
    .WithHttpsEndpoint(0123456789, name: "https") // for HTTP/1.1, HTTP/2
    .WithEndpoint("https-udp", e =>
    {
        e.Port = 0123456789;
        e.Transport = "http3";
        e.Protocol = ProtocolType.Udp;
        e.UriScheme = "https";
    })
    .WithUrlForEndpoint("https", e => e.Url = "https://localhost:0123456789/swagger")
    .WithUrlForEndpoint("https-udp", e => e.Url = "https://localhost:0123456789/swagger")
    .WithReference(db)
    .WaitFor(seeder);

await builder.Build().RunAsync();