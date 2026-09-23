var builder = DistributedApplication.CreateBuilder(args);

var clients = builder.AddProject<Projects.Bank_Clients>("clients")
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints();

var deposits = builder.AddProject<Projects.Bank_Deposits>("deposits")
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints()
    .WithEnvironment("ClientsApi__BaseUrl", clients.GetEndpoint("http"))
    .WaitFor(clients);

var credits = builder.AddProject<Projects.Bank_Credits>("credits")
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints()
    .WithEnvironment("ClientsApi__BaseUrl", clients.GetEndpoint("http"))
    .WithEnvironment("DepositsApi__BaseUrl", deposits.GetEndpoint("http"))
    .WaitFor(clients)
    .WaitFor(deposits);

var atm = builder.AddProject<Projects.Bank_Atm>("atm")
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints()
    .WithEnvironment("CreditsApi__BaseUrl", credits.GetEndpoint("http"))
    .WithEnvironment("DepositsApi__BaseUrl", deposits.GetEndpoint("http"))
    .WaitFor(credits)
    .WaitFor(deposits);

clients.WithEnvironment("DepositsApi__BaseUrl", deposits.GetEndpoint("http"));
clients.WithEnvironment("CreditsApi__BaseUrl", credits.GetEndpoint("http"));

var webfrontend = builder.AddViteApp("webfrontend", "../frontend");

var clientsfrontend = builder.AddViteApp("clientsfrontend", "../clients-frontend")
    .WithReference(clients)
    .WaitFor(clients)
    .WithEnvironment("SERVER_HTTP", clients.GetEndpoint("http"));

var depositsfrontend = builder.AddViteApp("depositsfrontend", "../deposits-frontend")
    .WithReference(deposits)
    .WaitFor(deposits)
    .WithEnvironment("SERVER_HTTP", deposits.GetEndpoint("http"));

var creditsfrontend = builder.AddViteApp("creditsfrontend", "../credits-frontend")
    .WithReference(credits)
    .WaitFor(credits)
    .WithEnvironment("SERVER_HTTP", credits.GetEndpoint("http"));

var atmfrontend = builder.AddViteApp("atmfrontend", "../atm-frontend")
    .WithReference(atm)
    .WaitFor(atm)
    .WithEnvironment("SERVER_HTTP", atm.GetEndpoint("http"));

webfrontend.WithEnvironment("VITE_CLIENTS_URL", clientsfrontend.GetEndpoint("http"));
webfrontend.WithEnvironment("VITE_DEPOSITS_URL", depositsfrontend.GetEndpoint("http"));
webfrontend.WithEnvironment("VITE_CREDITS_URL", creditsfrontend.GetEndpoint("http"));
webfrontend.WithEnvironment("VITE_ATM_URL", atmfrontend.GetEndpoint("http"));

clientsfrontend.WithEnvironment("VITE_HOME_URL", webfrontend.GetEndpoint("http"));
clientsfrontend.WithEnvironment("VITE_DEPOSITS_URL", depositsfrontend.GetEndpoint("http"));
clientsfrontend.WithEnvironment("VITE_CREDITS_URL", creditsfrontend.GetEndpoint("http"));
clientsfrontend.WithEnvironment("VITE_ATM_URL", atmfrontend.GetEndpoint("http"));

depositsfrontend.WithEnvironment("VITE_HOME_URL", webfrontend.GetEndpoint("http"));
depositsfrontend.WithEnvironment("VITE_CLIENTS_URL", clientsfrontend.GetEndpoint("http"));
depositsfrontend.WithEnvironment("VITE_CREDITS_URL", creditsfrontend.GetEndpoint("http"));
depositsfrontend.WithEnvironment("VITE_ATM_URL", atmfrontend.GetEndpoint("http"));

creditsfrontend.WithEnvironment("VITE_HOME_URL", webfrontend.GetEndpoint("http"));
creditsfrontend.WithEnvironment("VITE_CLIENTS_URL", clientsfrontend.GetEndpoint("http"));
creditsfrontend.WithEnvironment("VITE_DEPOSITS_URL", depositsfrontend.GetEndpoint("http"));
creditsfrontend.WithEnvironment("VITE_ATM_URL", atmfrontend.GetEndpoint("http"));

atmfrontend.WithEnvironment("VITE_HOME_URL", webfrontend.GetEndpoint("http"));
atmfrontend.WithEnvironment("VITE_CLIENTS_URL", clientsfrontend.GetEndpoint("http"));
atmfrontend.WithEnvironment("VITE_DEPOSITS_URL", depositsfrontend.GetEndpoint("http"));
atmfrontend.WithEnvironment("VITE_CREDITS_URL", creditsfrontend.GetEndpoint("http"));

clients.PublishWithContainerFiles(clientsfrontend, "wwwroot");
deposits.PublishWithContainerFiles(depositsfrontend, "wwwroot");
credits.PublishWithContainerFiles(creditsfrontend, "wwwroot");
atm.PublishWithContainerFiles(atmfrontend, "wwwroot");

builder.Build().Run();
