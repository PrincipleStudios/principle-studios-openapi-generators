# OpenAPI Codegen for a TypeScript-friendly RxJS client

Provides an adapter layer method for
[@principlestudios/openapi-codegen-typescript][1] to integrate with rxjs.

```sh
npm i -D @principlestudios/openapi-codegen-typescript-rxjs
```

You must also have `dotnet` 8.0 runtime installed on your machine.

This will provide a corresponding bin to generate the typescript files.

```sh
openapi-codegen-typescript api.yaml api-generated/ -c
```

You can then create an API wrapper such as:

```ts
import { toRxjsApi } from '@principlestudios/openapi-codegen-typescript-rxjs';
import operations from './api-generated/operations';

const baseDomain = 'http://localhost/';
export default toRxjsApi(operations, baseDomain);
```

To use within node (such as unit tests), use the `universal-rxjs-ajax` npm module:

```ts
import { toRxjsApi } from '@principlestudios/openapi-codegen-typescript-rxjs';
import operations from './api-generated/operations';
import { request as ajax } from 'universal-rxjs-ajax';

const baseDomain = 'http://localhost/';
export default toRxjsApi(operations, baseDomain, ajax);
```

This API will use the type safety from OpenAPI along with rxjs.

[1]: https://www.npmjs.com/package/@principlestudios/openapi-codegen-typescript