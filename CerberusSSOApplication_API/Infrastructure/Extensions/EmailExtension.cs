using Application.Email.Abstraction;
using Infrastructure.Email;
using Infrastructure.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class EmailExtension
{
    public static IServiceCollection AddEmailMessagingServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddKafkaProducer(config);
        services.AddSingleton<IEmailEventProducer, KafkaEmailEventProducer>();
        services.AddHostedService<KafkaEmailConsumerService>();

        services.Configure<SmtpOptions>(config.GetSection(SmtpOptions.SectionName));
        services.AddScoped<IEmailSender, EmailSender>();

        services.AddScoped<IEmailService, EmailService>();
        return services;
    }
}
