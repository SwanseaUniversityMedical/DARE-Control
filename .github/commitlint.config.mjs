import { RuleConfigSeverity } from '@commitlint/types';

export default {
  extends: ['@commitlint/config-conventional'],
  parserPreset: 'conventional-changelog-conventionalcommits',
  rules: {
    'scope-enum': [RuleConfigSeverity.Error, 'always', [
        '',
        'deps',
        'egress-api-container',
        'egress-ui-container',
        'main-api-container',
        'main-ui-container',
        'tre-api-container',
        'tre-ui-container',
        'tre-hasura-container',
        'tre-sql-pg-container',
        'tre-sql-trino-container'
    ]],
    'subject-case': [RuleConfigSeverity.Error, 'never', []],
  }
};
