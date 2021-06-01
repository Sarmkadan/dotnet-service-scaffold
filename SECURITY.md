# Security Policy

## Reporting a Vulnerability

**Do NOT open a public GitHub issue for security vulnerabilities.**

If you discover a security vulnerability, please report it privately to ensure it can be addressed before public disclosure.

### Reporting Methods

1. **Preferred: GitHub Private Vulnerability Reporting**
   - Visit: https://github.com/sarmkadan/dotnet-service-scaffold/security/advisories/new
   - Fill in vulnerability details
   - GitHub will coordinate with maintainers securely

2. **Alternative: Direct Email**
   - Email: rutova2@gmail.com
   - Subject line: `[SECURITY] Vulnerability Report`
   - Include detailed description and reproduction steps

### What to Include

When reporting a vulnerability, please provide:
- Description of the vulnerability
- Steps to reproduce
- Potential impact
- Suggested fix (if you have one)
- Your contact information

## Response Timeline

- **Initial Acknowledgment**: Within 48 hours
- **Assessment & Patch Development**: Within 1 week
- **Security Release**: Published when patch is ready
- **Disclosure**: Coordinated with reporter before public announcement

## Supported Versions

| Version | Status | Security Updates |
|---------|--------|------------------|
| 1.x     | Active | Yes              |
| 0.x     | EOL    | No               |

Only the latest version (1.x) receives security patches. Users are encouraged to upgrade to the latest version for security updates.

## Security Best Practices

When using dotnet-service-scaffold, follow these practices:

### 1. API Key Security
- Generate new API keys for each service/application
- Rotate keys regularly (quarterly minimum)
- Delete unused keys immediately
- Enable IP whitelisting for API keys
- Never commit API keys to version control
- Store keys in secure environment variables

### 2. Password Security
- Use strong passwords (minimum 12 characters)
- Include uppercase, lowercase, numbers, and symbols
- Never reuse passwords across systems
- Implement password managers for secure storage
- Change passwords after suspected compromises

### 3. HTTPS & TLS
- Always use HTTPS in production environments
- Use valid, trusted SSL certificates
- Enable HSTS headers
- Update TLS to latest secure version
- Disable insecure protocols (TLS 1.0, 1.1)

### 4. Database Security
- Use strong database passwords
- Enable database backups
- Store backups securely (encrypted)
- Restrict database access to necessary services only
- Regularly audit database access logs

### 5. Access Control
- Apply principle of least privilege
- Regularly review user permissions
- Disable accounts for inactive users
- Implement multi-factor authentication where possible
- Monitor account lockouts and failed login attempts

### 6. Monitoring & Logging
- Enable comprehensive audit logging
- Monitor audit logs for suspicious activity
- Set up alerts for security events
- Review logs regularly (daily minimum)
- Retain logs for investigation purposes

### 7. Deployment Security
- Run service with minimal required permissions
- Keep .NET runtime updated
- Apply OS security patches regularly
- Use firewall rules to restrict access
- Implement rate limiting for API endpoints

### 8. Configuration Security
- Never hardcode secrets in configuration files
- Use environment variables for sensitive values
- Encrypt sensitive configuration data
- Review appsettings files for exposed secrets
- Implement configuration management for production

## Vulnerability Disclosure

When a security vulnerability is fixed:
1. A security patch will be released as soon as possible
2. Security advisories will be published on GitHub
3. Reporters will be credited (unless anonymity is requested)
4. Users will be notified through available channels

## Third-Party Dependencies

This project uses third-party packages. To report security issues in dependencies:
- Report directly to the dependency maintainers
- Use their vulnerability reporting processes
- Notify us if a vulnerability affects dotnet-service-scaffold

## Questions?

For security-related questions or clarifications, email rutova2@gmail.com
