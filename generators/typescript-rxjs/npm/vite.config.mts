import { defineConfig } from 'vitest/config';
import tsconfigPaths from 'vite-tsconfig-paths';

// https://vitejs.dev/config/
export default defineConfig({
	plugins: [tsconfigPaths({ projects: ['./tsconfig.node.json'] })],
	test: {
		watch: false,
	},
});
