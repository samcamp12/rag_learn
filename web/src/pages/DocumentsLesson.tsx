import { useCallback, useEffect, useState } from 'react';
import { api, type DocumentSummary, type IngestResult } from '../api';

export function DocumentsLesson() {
  const [documents, setDocuments] = useState<DocumentSummary[]>([]);
  const [result, setResult] = useState<IngestResult | null>(null);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(() => {
    api.documents().then(setDocuments).catch((e: Error) => setError(e.message));
  }, []);

  useEffect(load, [load]);

  async function ingest() {
    setBusy(true);
    setError(null);
    try {
      setResult(await api.ingest());
      load();
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setBusy(false);
    }
  }

  return (
    <section>
      <div className="card">
        <div className="card-header">
          <h3>Sample Markdown files</h3>
          <button onClick={ingest} disabled={busy}>
            {busy ? 'Ingesting...' : 'Ingest sample docs'}
          </button>
        </div>
        {result && (
          <p className="success">
            Processed {result.documentsProcessed} files and created {result.chunksCreated} chunks.
          </p>
        )}
        {error && <p className="error">{error}</p>}

        {documents.length === 0 ? (
          <p className="muted">No documents yet. Click "Ingest sample docs" to load them into SQLite.</p>
        ) : (
          <table>
            <thead>
              <tr>
                <th>File</th>
                <th>Title</th>
                <th>Characters</th>
                <th>Chunks</th>
                <th>Ingested (UTC)</th>
              </tr>
            </thead>
            <tbody>
              {documents.map((d) => (
                <tr key={d.id}>
                  <td><code>{d.fileName}</code></td>
                  <td>{d.title}</td>
                  <td>{d.contentLength}</td>
                  <td>{d.chunkCount}</td>
                  <td>{new Date(d.ingestedAtUtc).toLocaleString()}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </section>
  );
}
