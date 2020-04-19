import React, { useState, useEffect, useMemo, useCallback } from 'react';
import './App.css';
import { DefaultApi, Pet, NewPet } from "./api-generated";

function App() {
  const [message, setMessage] = useState(null as (string | null));
  const [results, setResults] = useState([] as Pet[]);
  const updateResultsCallback = useCallback(updateResults, []);
  useEffect(() => { updateResultsCallback(); }, [updateResultsCallback]);
  const api = useMemo(() => new DefaultApi(), []);

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
            {results.map(r => <li key={r.id}>{r.name}</li>)}
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
    const result = await api.addPet({ newPet: pet }).toPromise();
    setMessage(`Added a ${result.tag}`);
    updateResultsCallback();
  }

  async function updateResults() {
    const results = await api.findPets({ tags: ['dog', 'cat'] }).toPromise();
    setResults(results);
  }
}

export default App;
