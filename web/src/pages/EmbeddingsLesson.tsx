import { useEffect, useState } from 'react';
import { api, type Status } from '../api';

export function EmbeddingsLesson() {
  const [status, setStatus] = useState<Status | null>(null);
  const [busy, setBusy] = useState(false);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const refresh = () => api.status().then(setStatus).catch((e: Error) => setError(e.message));

  useEffect(() => {
    refresh();
  }, []);

  async function embedAll() {
    setBusy(true);
    setError(null);
    setMessage(null);
    try {
      const result = await api.embed();
      setMessage(result.embedded === 0 ? 'Every chunk already has an embedding.' : `Embedded ${result.embedded} chunk(s).`);
      await refresh();
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setBusy(false);
    }
  }

  if (!status) {
    return error ? <p className="error">{error}</p> : <p className="muted">Loading status...</p>;
  }

  return (
    <section>
      {error && <p className="error">{error}</p>}
      <div className="card">
        <p>
          Embedded chunks: <strong>{status.embeddedChunkCount} / {status.chunkCount}</strong>
        </p>
        {!status.openAiConfigured && (
          <p className="error">The OpenAI key is not configured. Set it with user-secrets (see the README).</p>
        )}
        {status.chunkCount === 0 && <p className="muted">Ingest the documents in lesson 2 first.</p>}
        <button onClick={embedAll} disabled={busy || status.chunkCount === 0 || !status.openAiConfigured}>
          {busy ? 'Embedding...' : 'Embed all chunks'}
        </button>
        {message && <p className="muted">{message}</p>}
      </div>
    </section>
  );
}
