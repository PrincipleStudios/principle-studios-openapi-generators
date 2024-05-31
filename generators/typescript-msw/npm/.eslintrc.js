/** @type {import('eslint').Linter.Config} */
module.exports = {
	ignorePatterns: ['lib/', 'tests/*/**/*'],
	overrides: [
		{
			files: ['**/*.{ts,tsx}'],
			parserOptions: {
				ecmaVersion: 'latest',
				sourceType: 'module',
				project: './tsconfig.node.json',
				tsconfigRootDir: __dirname,
			},
		},
		{
			files: ['src/**/*.{ts,tsx}'],
			parserOptions: {
				ecmaVersion: 'latest',
				sourceType: 'module',
				project: './tsconfig.json',
				tsconfigRootDir: __dirname,
			},
		},
	],
};
