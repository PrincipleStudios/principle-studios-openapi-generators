/** @type {import('eslint').Linter.Config} */
module.exports = {
	ignorePatterns: ['lib/', 'tests/*/**/*'],
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
				'@typescript-eslint/no-explicit-any': 'off',
				'@typescript-eslint/ban-types': 'off',
			},
		},
		{
			files: ['src/**/*.{ts,tsx}'],
			parserOptions: {
				ecmaVersion: 'latest',
				sourceType: 'module',
				project: './tsconfig.build.json',
				tsconfigRootDir: __dirname,
			},
		},
	],
};
