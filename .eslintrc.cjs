/** @type {import('eslint').Linter.Config} */
module.exports = {
	root: true,
	plugins: ['@typescript-eslint', 'import'],
	extends: [
		// The order of these matter:
		// eslint baseline
		'eslint:recommended',
		// disables eslint rules in favor of using prettier separately
		'prettier',
	],
	rules: {
		// https://typescript-eslint.io/linting/troubleshooting/#i-get-errors-from-the-no-undef-rule-about-global-variables-not-being-defined-even-though-there-are-no-typescript-errors
		'no-undef': 'off',
	},
	ignorePatterns: [
		'artifacts/**/*',
		'**/generated/**/*',
		'*/dev-dist/**/*',
		'Server/wwwroot/assets/**/*',
	],
	parserOptions: {
		ecmaVersion: 'latest',
		sourceType: 'module',
	},
	overrides: [
		{
			files: ['**/*.{ts,tsx}'],
			extends: [
				// Recommended typescript changes, which removes some "no-undef" checks that TS handles
				'plugin:@typescript-eslint/eslint-recommended',
				'plugin:@typescript-eslint/recommended-type-checked',
				'plugin:@typescript-eslint/recommended',
			],
			rules: {
				'@typescript-eslint/array-type': ['error', { default: 'array-simple' }],
				'@typescript-eslint/consistent-type-imports': [
					'error',
					{
						disallowTypeAnnotations: false,
					},
				],
				// no-unsafe-assignment complains when passing components as variables
				'@typescript-eslint/no-unsafe-assignment': [0],
				// Adds naming conventions
				'@typescript-eslint/naming-convention': [
					'error',
					{
						selector: 'default',
						// React requires PascalCase for components, which can
						// be functions, variables, parameters, etc.
						format: ['camelCase', 'PascalCase'],
						leadingUnderscore: 'forbid',
					},
					{
						selector: 'class',
						format: ['PascalCase'],
					},
					{
						selector: 'classProperty',
						modifiers: ['private'],
						format: ['camelCase'],
						leadingUnderscore: 'require',
					},
					{
						selector: 'typeParameter',
						format: ['PascalCase'],
						prefix: ['T'],
					},
					{
						selector: 'typeAlias',
						format: ['PascalCase'],
					},
					{
						selector: 'interface',
						format: ['PascalCase'],
					},
					{
						selector: ['objectLiteralProperty', 'import'], // This effectively disables the rule for object literal properties and imports, which we do not always control
						format: null,
					},
				],
				'import/order': [
					'error',
					{
						pathGroupsExcludedImportTypes: ['react'],
						'newlines-between': 'never',
						alphabetize: {
							order: 'asc',
							caseInsensitive: true,
						},
						groups: [
							'external',
							'builtin',
							'internal',
							'parent',
							'sibling',
							'index',
							'object',
							'unknown',
						],
						pathGroups: [
							{
								pattern: 'react**',
								group: 'external',
								position: 'before',
							},
							{
								pattern: '@!(/)*/**',
								group: 'external',
								position: 'before',
							},
							{
								pattern: '@/**',
								group: 'internal',
								position: 'before',
							},
						],
					},
				],
			},
		},
	],
};
