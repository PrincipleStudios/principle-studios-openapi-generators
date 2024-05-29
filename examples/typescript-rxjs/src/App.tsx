import React, { useState, useEffect, useMemo, useCallback } from 'react';
import './App.css';
import operations from "./api-generated/operations";
import { toRxjsApi } from '@principlestudios/openapi-codegen-typescript-rxjs';
import { Pet } from './api-generated/models/Pet';
import { NewPet } from './api-generated/models/NewPet';

function App() {
  const [message, setMessage] = useState(null as (string | null));
  const [results, setResults] = useState([] as Pet[]);
  const updateResultsCallback = useCallback(updateResults, []);
  useEffect(() => { updateResultsCallback(); }, [updateResultsCallback]);
  const api = useMemo(() => toRxjsApi(operations, 'https://localhost:5001'), []);

  return (
    <div className="App">
      <header className="App-header">
        <button
          onClick={addDog}
        >
          Add a dog
        </button>

        <button
          onClick={addCat}
        >
          Add a cat
        </button>

        {message && <p>{message}</p>}

        <h1>Results</h1>
        {results.length === 0 ? "None"
          : <ul>
            {results.map(r => <li key={`${r.id}`}>{r.name}</li>)}
          </ul>}
      </header>
    </div>
  );

  async function addDog() {
    return addPet({ name: 'Fido', tag: 'dog' });
  }

  async function addCat() {
    return addPet({ name: 'Spaz', tag: 'cat' });
  }

  async function addPet(pet: NewPet) {
    const result = await api.addPet({ body: pet, mimeType: 'application/json' }).toPromise();
    if (result.statusCode === 200) {
      setMessage(`Added a ${result.data.tag}`);
      updateResultsCallback();
    }
  }

  async function updateResults() {
    const results = await api.findPets({ params: { tags: ['dog', 'cat'] } }).toPromise();
    if (results.statusCode === 200)
    {
      setResults(results.data);
    }
  }
}

export default App;
