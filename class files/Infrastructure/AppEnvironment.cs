using Microsoft.Extensions.FileProviders;

namespace AppEnvironment
{
    public class AppEnvironment
    {
        /// <summary>
        /// Gets or sets the name of the environment. 
        /// </summary>
        public string EnvironmentName { get; init; }

        /// <summary>
        /// Gets or sets the name of the application. 
        /// </summary>
        public string ApplicationName { get; init; }

        /// <summary>
        /// Gets or sets the absolute path to the directory that contains the application content files.
        /// </summary>
        public string ContentRootPath { get; init; }

        /// <summary>
        /// Gets or sets an <see cref="IFileProvider"/> pointing at <see cref="ContentRootPath"/>.
        /// </summary>
        public IFileProvider ContentRootFileProvider { get; init; }

        /// <summary>
        /// Gets or sets the absolute path to the directory that contains the web-servable application content files.
        /// </summary>
        public string WebRootPath { get; init; }

        /// <summary>
        /// Gets or sets an <see cref="IFileProvider"/> pointing at <see cref="WebRootPath"/>.
        /// </summary>
        public IFileProvider WebRootFileProvider { get; init; }

        public bool IsWebHost { get; init; }

        public bool IsDevelopment => IsEnvironment("Development");

        public bool IsStaging => IsEnvironment("Staging");

        public bool IsProduction => IsEnvironment("Production");

        public bool IsEnvironment(string environmentName) => string.Equals(this.EnvironmentName, environmentName, StringComparison.OrdinalIgnoreCase);

    }
}
