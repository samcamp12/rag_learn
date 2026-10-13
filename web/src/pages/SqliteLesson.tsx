import { useState } from 'react';
import { api, type SqlResult } from '../api';

const examples: { label: string; sql: string }[] = [
  { label: 'List tables', sql: "SELECT name FROM sqlite_master WHERE type = 'table';" },
  { label: 'Documents', sql: 'SELECT Id, FileName, Title, length(Content) AS Chars FROM Documents;' },
  {
    label: 'Chunks per document',
    sql: 'SELECT d.FileName, COUNT(c.Id) AS Chunks\nFROM Documents d\nLEFT JOIN Chunks c ON c.DocumentId = d.Id\nGROUP BY d.FileName\nORDER BY Chunks DESC;',
  },
  {
    label: 'Largest chunks',
    sql: 'SELECT DocumentId, Position, Heading, CharCount\nFROM Chunks\nORDER BY CharCount DESC\nLIMIT 5;',
  },
  { label: 'Table schema', sql: "PRAGMA table_info('Chunks');" },
];

export function SqliteLesson() {
  const [sql, setSql] = useState(examples[0].sql);
  const [result, setResult] = useState<SqlResult | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  async function run() {
    setBusy(true);
    setError(null);
    try {
      setResult(await api.runSql(sql));
    } catch (e) {
      setResult(null);
      setError((e as Error).message);
    } finally {
      setBusy(false);
    }
  }

  return (
    <section>
      <div className="card">
        <h3>Read-only SQL console</h3>
        <p className="muted">
          The API opens the SQLite file in read-only mode, so you can experiment safely.
          Try an <code>INSERT</code> or <code>DELETE</code> and observe the error.
        </p>

        <div className="chips">
          {examples.map((ex) => (
            <button key={ex.label} className="chip" onClick={() => setSql(ex.sql)}>
              {ex.label}
            </button>
          ))}
        </div>

        <textarea
          className="sql-input"
          value={sql}
          onChange={(e) => setSql(e.target.value)}
          spellCheck={false}
          rows={6}
        />
        <button onClick={run} disabled={busy}>
          {busy ? 'Running...' : 'Run query'}
        </button>

        {error && <p className="error">{error}</p>}

        {result && (
          <div className="table-scroll">
            <p className="muted">{result.rows.length} row(s)</p>
            <table>
              <thead>
                <tr>
                  {result.columns.map((c) => (
                    <th key={c}>{c}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {result.rows.map((row, i) => (
                  <tr key={i}>
                    {row.map((value, j) => (
                      <td key={j}>{value === null ? <em>NULL</em> : String(value)}</td>
                    ))}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </section>
  );
}
