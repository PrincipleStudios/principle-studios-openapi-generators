/** @type {import('eslint').Linter.Config} */
module.exports = {
	ignorePatterns: ['*.yaml/**/*'],
	overrides: [
		{
			files: ['**/*.{ts,tsx}'],
			parserOptions: {
				ecmaVersion: 'latest',
				sourceType: 'module',
				project: './tsconfig.json',
				tsconfigRootDir: __dirname,
			},
			rules: {
				'import/order': 'off',
			},
		},
	],
};
