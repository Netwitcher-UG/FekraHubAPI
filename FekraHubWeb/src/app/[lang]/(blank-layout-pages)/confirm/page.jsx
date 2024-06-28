// Component Imports
import Confirm from '@views/Confirm'

// Server Action Imports
import { getServerMode } from '@core/utils/serverHelpers'

export const metadata = {
  title: 'Confirm',
  description: 'Confirm your account'
}

const ConfirmPage = () => {
  // Vars
  const mode = getServerMode()

  return <Confirm mode={mode} />
}

export default ConfirmPage
