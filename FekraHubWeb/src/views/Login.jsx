'use client'

// React Imports
import { useState } from 'react'

// Next Imports
import { useRouter } from 'next/navigation'

// MUI Imports
import Typography from '@mui/material/Typography'
import TextField from '@mui/material/TextField'
import IconButton from '@mui/material/IconButton'
import InputAdornment from '@mui/material/InputAdornment'
import Checkbox from '@mui/material/Checkbox'
import Button from '@mui/material/Button'
import FormControlLabel from '@mui/material/FormControlLabel'
import Alert from '@mui/material/Alert'

// Third-party Imports
import { Controller, useForm } from 'react-hook-form'
import { valibotResolver } from '@hookform/resolvers/valibot'
import { object, minLength, string, email } from 'valibot'

// Component Imports
import axios from 'axios'

import Logo from '@core/svg/Logo'
import Illustrations from '@components/Illustrations'

// Config Imports
import themeConfig from '@configs/themeConfig'

// Hook Imports
import { useImageVariant } from '@core/hooks/useImageVariant'

const Login = ({ mode }) => {
  // States
  const [isPasswordShown, setIsPasswordShown] = useState(false)
  const [errorState, setErrorState] = useState(null)

  // Hooks
  const router = useRouter()

  const fields = [
    { name: 'email', label: 'Email' },
    { name: 'password', label: 'Password', type: 'password' },
  ]

  const [formData, setFormData] = useState({
    password: '',
    email: ''
  })

  const handleChange = (name, value) => {
    setFormData(prevData => ({
      ...prevData,
      [name]: value
    }))
  }

  const handleRegister = async e => {
    e.preventDefault()

    try {
      const formDataToSend = new FormData()

      for (const key in formData) {
        formDataToSend.append(key, formData[key])
      }

      const res = await axios.post('https://localhost:7288/api/Account/LogIn', formDataToSend)

      console.log(res)

      router.push('/dashboards/crm')

      console.log('Registration successful')
    } catch (error) {
      console.error('Error registering user:', error)
    }
  }

  const authBackground = useImageVariant(
    mode,
    '/images/pages/auth-v2-mask-light.png',
    '/images/pages/auth-v2-mask-dark.png'
  )

  const characterIllustration = useImageVariant(
    mode,
    '/images/illustrations/auth/v2-login-light.png',
    '/images/illustrations/auth/v2-login-dark.png',
    '/images/illustrations/auth/v2-login-light-border.png',
    '/images/illustrations/auth/v2-login-dark-border.png'
  )

  const handleClickShowPassword = () => {
    setIsPasswordShown(prev => !prev)
  }

  return (
    <>
      <div className='flex bs-full justify-center'>
        <div className='flex bs-full items-center justify-center flex-1 min-bs-[100dvh] relative p-6 max-md:hidden'>
          <div className='plb-12 pis-12'>
            <img
              src={characterIllustration}
              alt='character-illustration'
              className='max-bs-[500px] max-is-full bs-auto'
            />
          </div>
          <Illustrations
            image1={{ src: '/images/illustrations/objects/tree-2.png' }}
            image2={null}
            maskImg={{ src: authBackground }}
          />
        </div>
        <div className=' justify-center items-center bs-full bg-backgroundPaper !min-is-full p-6 md:!min-is-[unset] md:p-12 md:is-[480px]'>
          <div className='absolute block-start-5 sm:block-start-[33px] inline-start-6 sm:inline-start-[38px]'>
            <div className='flex justify-center items-center gap-3 mbe-6'>
              <Logo className='text-primary' height={28} width={35} />
              <Typography variant='h4' className='font-semibold tracking-[0.15px]'>
                {themeConfig.templateName}
              </Typography>
            </div>
          </div>
          <div className='flex flex-col gap-5 is-full sm:is-auto md:is-full sm:max-is-[400px] md:max-is-[unset]'>
            <div>
              <Typography variant='h4'>{`Welcome to ${themeConfig.templateName}!👋🏻`}</Typography>
              <Typography>Please sign-in to your account and start the adventure</Typography>
            </div>
            {errorState && (
              <Alert severity='error'>
                <Typography>{errorState.message}</Typography>
              </Alert>
            )}
          </div>
          <div className='login-form mt-10'>
            <form noValidate autoComplete='off' onSubmit={handleRegister} className='flex flex-col gap-5'>
              {fields.map(field => (
                <div key={field.name}>
                  {
                    <TextField
                      value={formData[field.name]}
                      onChange={e => handleChange(field.name, e.target.value)}
                      fullWidth
                      label={field.label}
                      type={field.type === 'password' && !isPasswordShown ? 'password' : 'text'}
                      name={field.name}
                      InputProps={
                        field.name === 'password'
                          ? {
                              endAdornment: (
                                <InputAdornment position='end'>
                                  <IconButton
                                    edge='end'
                                    onClick={handleClickShowPassword}
                                    onMouseDown={e => e.preventDefault()}
                                  >
                                    {isPasswordShown ? (
                                      <i className='ri-eye-off-line' />
                                    ) : (
                                      <i className='ri-eye-line' />
                                    )}
                                  </IconButton>
                                </InputAdornment>
                              )
                            }
                          : null
                      }
                    />
                  }
                </div>
              ))}
              <div className='flex justify-between items-center flex-wrap gap-x-3 gap-y-1'>
                <FormControlLabel control={<Checkbox defaultChecked />} label='Remember me' />
                <Typography className='text-end' color='primary' component='a' href='./forgot-password'>
                  Forgot password?
                </Typography>
              </div>
              <Button fullWidth variant='contained' type='submit'>
                Log In
              </Button>
              <div className='flex justify-center items-center flex-wrap gap-2'>
                <Typography>New on our platform?</Typography>
                <Typography component='a' href='/register' color='primary'>
                  Create an account
                </Typography>
              </div>
            </form>
          </div>
        </div>
      </div>
    </>
  )
}

export default Login
