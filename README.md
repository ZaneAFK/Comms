# Comms Project

Comms is a simple and secure web-based messaging application built for those who want privacy.

## How to Deploy

Run the installer script to start the latest release of Comms.

```bash
curl -fsSL https://raw.githubusercontent.com/ZaneAFK/Comms/master/install.sh | sudo bash
```

The app will be available at `http://<your-server>:8002`.

### Updating

Re-run the same command. It reuses your existing secrets, pulls the newest release and restarts the service.

### Pinning a version or install directory

Use `--dir` to install somewhere other than `/etc/comms`.

Use `--version` to pull a explicit version of Comms.

```bash
curl -fsSL https://raw.githubusercontent.com/ZaneAFK/Comms/master/install.sh | bash -s -- --version 1.2.0 --dir /opt/comms
```

> **HTTPS:** The default config serves HTTP only. To add HTTPS, either put your own reverse proxy (Caddy, Traefik, etc.) in front of the stack, or swap in an HTTPS-capable `nginx.conf` with a `certbot` container alongside.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for setup instructions and the suggested development workflow.
