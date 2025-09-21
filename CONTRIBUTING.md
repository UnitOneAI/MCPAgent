# Contributing to Windows 365 MCP Server

Thank you for your interest in contributing to the Windows 365 MCP Server! This project is part of UNITONE's commitment to making enterprise AI workloads visible, controllable, and safe.

## 🚀 Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Git](https://git-scm.com/downloads)
- Entra ID tenant with Windows 365 licenses
- Entra ID App Registration with required permissions

### Development Setup

1. **Fork and Clone**
   ```bash
   git clone https://github.com/your-username/windows-365-mcp-server.git
   cd windows-365-mcp-server
   ```

2. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure Authentication**
   - Set environment variables: `AZURE_TENANT_ID`, `AZURE_CLIENT_ID`, and `AZURE_CLIENT_SECRET`
   - The server uses Azure Identity DefaultAzureCredential with STDIO transport

4. **Build and Run**
   ```bash
   dotnet build
   dotnet run --project src/Windows365.Mcp.Server
   ```

## 🛠️ Development Guidelines

### Code Style

- Follow standard C# conventions and .NET best practices
- Use meaningful variable and method names
- Include XML documentation for public APIs
- Keep methods focused and concise (ideally < 50 lines)

### Project Structure

```
src/Windows365.Mcp.Server/
├── Prompts/           # MCP prompt implementations
├── Resources/         # MCP resource handlers
├── Services/          # Graph API service layer
├── Tools/             # MCP tool implementations
├── GlobalUsings.cs    # Global using directives
└── Program.cs         # Application entry point
```

### Microsoft Graph API Guidelines

- **Always verify endpoints** against [official documentation](https://learn.microsoft.com/en-us/graph/api/resources/cloudpc-api-overview)
- **Use correct permissions** - prefer least privilege
- **Handle rate limiting** with exponential backoff
- **Log all operations** for audit trails
- **Cache responses** when appropriate with proper TTL

### Git Workflow

1. **Create a feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes**
   - Follow the coding standards
   - Add tests for new functionality
   - Update documentation as needed

3. **Commit with clear messages**
   ```bash
   git commit -m "feat: add support for bulk Cloud PC provisioning"
   ```

4. **Push and create PR**
   ```bash
   git push origin feature/your-feature-name
   ```

### Commit Message Format

We follow [Conventional Commits](https://www.conventionalcommits.org/):

- `feat:` - New features
- `fix:` - Bug fixes
- `docs:` - Documentation changes
- `style:` - Code style changes (formatting, etc.)
- `refactor:` - Code refactoring
- `test:` - Adding or updating tests
- `chore:` - Maintenance tasks

## 🧪 Testing

The project currently focuses on core MCP server functionality. Test coverage will be added in future releases. To verify the server works correctly:

```bash
# Build and run the server
dotnet build
dotnet run --project src/Windows365.Mcp.Server

# Test with MCP Inspector (in another terminal)
npx @modelcontextprotocol/inspector dotnet run --project src/Windows365.Mcp.Server
```

## 📝 Documentation

### Code Documentation

- Add XML documentation for all public methods
- Include parameter descriptions and return values
- Document any exceptions that may be thrown

```csharp
/// <summary>
/// Retrieves Cloud PCs for a specific user
/// </summary>
/// <param name="userId">The user's UPN or object ID</param>
/// <returns>Collection of Cloud PCs assigned to the user</returns>
/// <exception cref="UnauthorizedAccessException">Thrown when insufficient permissions</exception>
public async Task<IEnumerable<CloudPC>> GetUserCloudPCsAsync(string userId)
```

### README Updates

- Update the README.md if adding new features
- Include usage examples for new tools
- Update configuration instructions if needed

## 🐛 Bug Reports

When reporting bugs, please include:

1. **Environment details** (.NET 9.0, OS, Azure tenant type)
2. **Steps to reproduce** the issue
3. **Expected vs actual behavior**
4. **Error messages** and stack traces
5. **Sample code** if applicable

## 💡 Feature Requests

For new features, please:

1. **Check existing issues** to avoid duplicates
2. **Describe the use case** and business value
3. **Provide implementation suggestions** if you have them
4. **Consider backward compatibility**

## 🔒 Security

- **Never commit secrets** or credentials
- **Use secure authentication flows** (Azure Identity with STDIO transport)
- **Follow least privilege principle** for permissions
- **Report security issues** privately to security@unitone.ai

## 📋 Pull Request Checklist

Before submitting a PR, ensure:

- [ ] Code follows project style guidelines
- [ ] Code builds successfully with `dotnet build`
- [ ] MCP server runs correctly with `dotnet run`
- [ ] Documentation is updated
- [ ] Commit messages follow conventional format
- [ ] No secrets or credentials in code
- [ ] PR description clearly explains changes

## 🌟 Recognition

Contributors will be:

- Added to the project's contributor list
- Mentioned in release notes for significant contributions
- Invited to join the UNITONE community Discord

## 📞 Support

- **General questions**: [GitHub Discussions](https://github.com/unitone-ai/windows-365-mcp-server/discussions)
- **Bug reports**: [GitHub Issues](https://github.com/unitone-ai/windows-365-mcp-server/issues)
- **Security issues**: security@unitone.ai
- **Community**: [UNITONE Discord](https://discord.gg/EMcCcMc9)

## 📄 License

By contributing, you agree that your contributions will be licensed under the MIT License.

---

Thank you for helping make Windows 365 management more accessible and automated! 🚀