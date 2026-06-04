import { spawn } from 'node:child_process';
import { createRequire } from 'node:module';

const require = createRequire(import.meta.url);
const ngCli = require.resolve('@angular/cli/bin/ng');
const port = process.env.PORT ?? '4200';

const child = spawn(
  process.execPath,
  [ngCli, 'serve', '--port', port, '--proxy-config', 'proxy.conf.cjs'],
  {
    stdio: 'inherit',
    env: process.env,
  },
);

child.on('exit', code => {
  process.exit(code ?? 0);
});
