import { useCallback, useEffect, useState } from 'react';
import { api, type Status } from '../api';

export function OverviewLesson() {
  const [status, setStatus] = useState<Status | null>(null);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(() => {
    setError(null);
    api.status().then(setStatus).catch((e: Error) => setError(e.message));
  }, []);

  useEffect(load, [load]);

  return (
    <section>
      <p>
        This app has three parts: a React frontend, an ASP.NET Core API, and a SQLite file on disk.
        The status below comes from the API and reads the SQLite database.
      </p>

      <div className="card">
        <div className="card-header">
          <h3>Backend status</h3>
          <button onClick={load}>Refresh</button>
        </div>
        {error && <p className="error">{error}</p>}
        {status && (
          <dl className="kv">
            <dt>Database file</dt>
            <dd><code>{status.databasePath}</code></dd>
            <dt>Documents folder</dt>
            <dd><code>{status.docsPath}</code></dd>
            <dt>Documents in DB</dt>
            <dd>{status.documentCount}</dd>
            <dt>Chunks in DB</dt>
            <dd>{status.chunkCount}</dd>
            <dt>OpenAI key set</dt>
            <dd>{status.openAiConfigured ? 'Yes' : 'No (paste it with user-secrets, see Instructions)'}</dd>
          </dl>
        )}
      </div>
    </section>
  );
}
