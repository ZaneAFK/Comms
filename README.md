# Comms Project

Comms is a simple and secure web-based messaging application built for those who want privacy.

## How to Deploy

Run the installer script to start the latest release of Comms.

```bash
curl -fsSL https://raw.githubusercontent.com/ZaneAFK/Comms/master/install.sh | sudo bash
```

This creates an `/etc/comms` directory with everything the stack needs and runs `docker compose up -d`. The app will be available at `http://<your-server>:8002`.

Secrets (`DATABASE_PASSWORD`, `JWT_KEY`) are generated automatically and saved to `/etc/comms/.env` (mode `600`) — back this file up, it's not shown again.

### Updating

Re-run the same command. It reuses your existing `.env` secrets, refreshes `compose.yml`/`nginx.conf`, bumps `COMMS_VERSION` to the newest release, and pulls + restarts the stack.

### Pinning a version or install directory

Use `--dir` to install somewhere other than `/etc/comms` — useful if you don't have or don't want to use root:

```bash
curl -fsSL https://raw.githubusercontent.com/ZaneAFK/Comms/master/install.sh | bash -s -- --version 1.2.0 --dir /opt/comms
```f

### Installing without piping to bash

If you'd rather inspect the script first:

```bash
curl -fsSL https://raw.githubusercontent.com/ZaneAFK/Comms/master/install.sh -o install.sh
less install.sh
bash install.sh
```

Or do it fully by hand: download `compose.yml`, `nginx.conf` and `.env.template` from the [latest release](../../releases/latest), copy `.env.template` to `.env` and fill in the values, then run `docker compose up -d`.

> **HTTPS:** The default config serves HTTP only. To add HTTPS, either put your own reverse proxy (Caddy, Traefik, etc.) in front of the stack, or swap in an HTTPS-capable `nginx.conf` with a `certbot` container alongside.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for setup instructions and the suggested development workflow.
